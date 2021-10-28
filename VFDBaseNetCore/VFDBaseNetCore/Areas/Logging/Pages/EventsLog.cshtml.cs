using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;
using MTF.Utilities;

using MTF.Areas.Logging.Data;
using MTF.Areas.Logging.Models;

namespace MTF.Utilities
{
    public partial class pageParameters
    {
        public string CurrentStartDate { get; set; }
        public string CombinedFilter_filter { get; set; }
        public int CombinedFilter_updated { get; set; }
    }
}

namespace MTF.Areas.Logging.Pages
{
    [Authorize]
    public class EventsLogModel : Ext_AuthPageModel
    {
        private LogDB_Context _LogDB_Context { get; set; }
        public List<SelectListItem> AvailableStartDates { get; set; }
        public List<SelectListItem> AvailableSearchFields { get; set; }

        public EventsLogModel(CommonIdent context,
                              IAuthorizationService authorizationService,
                              UserManager<CommonUser> userManager,
                              ILogger<EventsLogModel> logger,
                              LogDB_Context LogDB_Context,
                              IpSafeList ipSafeList
                             )
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Events Log"
                  )
        {
            _LogDB_Context = LogDB_Context;
            
            AvailableStartDates =
                new List<SelectListItem>(new[] { new SelectListItem { Value = "1", Text = "Last Hour"},
                                                 new SelectListItem { Value = "3", Text = "Last 3 Hours"},
                                                 new SelectListItem { Value = "12", Text = "Last 12 Hours"},
                                                 new SelectListItem { Value = "24", Text = "Last 24 Hours"},
                                                 new SelectListItem { Value = "168", Text = "Last Week"},
                                                 new SelectListItem { Value = "720", Text = "Last 30 Days"}
                                               }
                                        );
            AvailableSearchFields =
                new List<SelectListItem>(new[] { new SelectListItem { Value = "appIdent", Text = "appIdent"},
                                                 new SelectListItem { Value = "machineName", Text = "machineName"},
                                                 new SelectListItem { Value = "level", Text = "level"},
                                                 new SelectListItem { Value = "message", Text = "message"},
                                                 new SelectListItem { Value = "logger", Text = "logger"},
                                                 new SelectListItem { Value = "exception", Text = "exception"},
                                                 new SelectListItem { Value = "callsite", Text = "callsite"}
                                               }
                                        );
            this.CombinedFilter = new combinedFilter (GlobalParameters._combinedFiltersTemplates.EventsLog, 
                                                      0 );
        }

        // Idea to keep parameters for POST routine here
        // BUT! In this case we don't use any return of data array, as such
        // let's comment the following line
        //[BindProperty]
        public List<GeneralLog_v> GeneralLogPage { get; set; }

        public class combinedFilter
        {
            public string _CombinedFilter { get; set; }
            public int _updated { get; set; }
            public combinedFilter(combinedFilter _cp)
            {
                _CombinedFilter = _cp._CombinedFilter;
                _updated = _cp._updated;
            }
            public combinedFilter(string str, int upd)
            {
                _CombinedFilter = str;
                _updated = upd;
            }
            public combinedFilter()
            {
                _CombinedFilter = GlobalParameters._combinedFiltersTemplates.EventsLog;
                _updated = 0;
            }
        }
        // BUT we need to update combined search string, that's why:
        [BindProperty]
        public combinedFilter CombinedFilter { get; set; }

        // idea to keep here all parameters for page html generation
        public pageParameters pgParms { get; set; }
        public string PagingNotification { get; set; }
        public CommonUser currentUser { get; set; }

        public async Task<IActionResult> OnGetAsync(string haveError,
                                                    string fromPOST,
                                                    string workFlowId,
                                                    string startDate,
                                                    string prevStartDate,
                                                    string newSortOrder,
                                                    string prevSortOrder,
                                                    string prevSortDirection,
                                                    string searchString,
                                                    string searchField,
                                                    int? pageIndex,
                                                    int? pageSize,
                                                    int? clearing)
        {
            try
            {
                currentUser = await verifySUwr();
                if (currentUser == null)
                {
                    return NotFound();
                }

                if (haveError == "Yes")
                {
                    setEmptyPage(workFlowId);
                    TempData.Put("pgParms-EventsLog", pgParms);
                    TempData.Keep("pgParms-EventsLog");

                    return await Task.FromResult(Page());
                }

                pgParms = TempData.Get<pageParameters>("pgParms-EventsLog");
                if ((pgParms != null) ? (pgParms.uid != currentUser.Id) : true)
                {
                    var _cmbFilter = (await _LogDB_Context.getScalarValue("call eventsLogCache_GetCmbFilter",
                                                                                                 currentUser.Id,
                                                                                                 GlobalParameters._combinedFiltersTemplates.EventsLog
                                                                                                 )
                                                            ).RetValueString;
                    pgParms = new pageParameters
                    {
                        uid = currentUser.Id,
                        CurrentPageSize = GlobalParameters.DefaultPageSize,
                        CombinedFilter_filter = _cmbFilter,
                        CombinedFilter_updated = 1
                    };
                }

                if (pgParms.CombinedFilter_filter == null ||
                    pgParms.CombinedFilter_filter == "" ||
                    clearing == 1
                   )
                {
                    pgParms.CombinedFilter_filter = GlobalParameters._combinedFiltersTemplates.EventsLog;
                    pgParms.CombinedFilter_updated = 1;
                }
                this.CombinedFilter._CombinedFilter = pgParms.CombinedFilter_filter;
                this.CombinedFilter._updated = pgParms.CombinedFilter_updated;

                // save value for combined filter
                if (this.CombinedFilter._updated == 1)
                {
                    var rc = (await _LogDB_Context.getScalarValue("call eventsLogCache_UpdateCmbFilter",
                                                                                         currentUser.Id,
                                                                                         this.CombinedFilter._CombinedFilter
                                                                                         )
                                                    ).RetValueInt;
                }
                                
                pgParms.CombinedFilter_updated = 0; // previous state is in this.CombinedFilter._updated
                TempData.Keep("pgParms-EventsLog");

                // Start date choice
                pgParms.CurrentStartDate = startDate ?? "1"; //"Last 1 Hour";
                                    
                // workflow processing The trick to recognize worfklow
                // i.e. serial of page insantiations in the same fuctional
                // entity
                if (workFlowId == null)
                {
                    // some logic for workflow initialization
                    var wf = Guid.NewGuid().ToString();
                    pgParms.WorkFlowId = wf + "a";
                }
                else
                {
                    pgParms.WorkFlowId = workFlowId;
                }

                // Work with sorting by fields
                if (newSortOrder == null)
                {
                    newSortOrder = "sortA";
                    prevSortOrder = "sortA";
                    pgParms.SortDirection = "_";
                }
                if (newSortOrder != prevSortOrder)
                {
                    pgParms.SortDirection = "_";
                }
                else
                {
                    prevSortDirection ??= "_desc";
                    pgParms.SortDirection = prevSortDirection == "_desc" ? "_" : "_desc";
                }
                pgParms.CurrentSort = newSortOrder;

                // Work with search string
                pgParms.CurrentSearchField = searchField ?? (pgParms.CurrentSearchField ?? "appIdent");
                pgParms.CurrentFilter = searchString;
                searchString = String.IsNullOrEmpty(searchString) ? "%" : searchString;

                // Work with paginated data
                var _ps = pageSize ?? pgParms.CurrentPageSize;
                pgParms.CurrentPageSize = _ps;
                var _pi = pageIndex ?? 1;
                var recStart = (_pi - 1) * _ps + 1;
                var recEnd = _pi * _ps;

                // do we need to build a new cache for current user?
                if (
                        workFlowId == null ||
                        startDate == null ||
                        startDate != prevStartDate
                    )
                {
                    // Building identities cache for current user
                    var rc01 = (await _LogDB_Context.getScalarValue("call eventsLogCache_Drop",
                                                                                            currentUser.Id,
                                                                                            pgParms.WorkFlowId
                                                                                            ));
                    var rc02 = (await _LogDB_Context.getScalarValue("call eventsLogCache_Build",
                                                                                            currentUser.Id,
                                                                                            pgParms.WorkFlowId,
                                                                                            pgParms.CurrentStartDate
                                                                                            ));
                }

                // Get the page of Data itself
                GeneralLogPage = (await _LogDB_Context.generalLog_v
                                                                .FromSqlRaw("call eventsLogCache_GetPage_wcmpflt01 ({0},{1},{2},{3}, {4},{5}, {6});",
                                                                            currentUser.Id,
                                                                            searchString,
                                                                            pgParms.CurrentSort,
                                                                            pgParms.SortDirection,
                                                                            _pi, _ps,
                                                                            pgParms.CurrentSearchField)
                                                                .AsNoTracking()
                                                                .ToListAsync());

                // Find total number of records
                var count = GeneralLogPage.Count>0 ? GeneralLogPage.First().numRecs : 0;
                
                recEnd = recEnd > count ? count : recEnd;
                PagingNotification = "recs: " + recStart.ToString() + "-" + recEnd.ToString() + " of " + count.ToString();

                pgParms.PageIndex = _pi;
                var tp = (int)Math.Ceiling(count / (double)_ps);
                pgParms.TotalPages = tp <= 0 ? 1 : tp;

                if (_pi < 1 || _pi > pgParms.TotalPages)
                {
                    throw new Exception("Page number out of possible range.") ;
                }

                // set not-updated stae for post-variable
                CombinedFilter._updated = 0;
                pgParms.CombinedFilter_updated = 0;

                TempData.Put("pgParms-EventsLog", pgParms);
                TempData.Keep("pgParms-EventsLog");

                return await Task.FromResult(Page());
            }
            catch (Exception ex)
            {
                _statusMessage = "Error: "
                                 + ex.Message
                                 + "; "
                                 + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                _logger.LogError($"{_statusMessage}.");

                setEmptyPage(workFlowId);
                TempData.Put("pgParms-EventsLog", pgParms);
                TempData.Keep("pgParms-EventsLog");

                return await Task.FromResult(RedirectToPage("./EventsLog",
                                                            new { haveError = "Yes",
                                                                  workFlowId = pgParms.WorkFlowId
                                                                }
                                                           )
                                            );
            }          
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                CommonUser currentUser = await verifySUwr();
                if (currentUser == null)
                {
                    return NotFound();
                }

                pgParms = TempData.Get<pageParameters>("pgParms-EventsLog");
                if (pgParms == null)
                {
                    pgParms = new pageParameters
                    {
                        CurrentPageSize = GlobalParameters.DefaultPageSize
                    };
                }
                TempData.Keep("pgParms-EventsLog");

                string invSortDirection = pgParms.SortDirection ?? "_desc";
                invSortDirection = invSortDirection == "_desc" ? "_" : "_desc";

                // extract post-variable
                pgParms.CombinedFilter_updated = CombinedFilter._updated;
                pgParms.CombinedFilter_filter = CombinedFilter._CombinedFilter;

                TempData.Put("pgParms-EventsLog", pgParms);
                TempData.Keep();
                return await Task.FromResult(RedirectToPage("./EventsLog",
                                                            new { fromPOST = "Yes",
                                                                  workFlowId = pgParms.WorkFlowId,
                                                                  newSortOrder = pgParms.CurrentSort,
                                                                  prevSortOrder = pgParms.CurrentSort,
                                                                  prevSortDirection = invSortDirection,
                                                                  searchString = pgParms.CurrentFilter,
                                                                  pageIndex = pgParms.PageIndex,
                                                                  pageSize = pgParms.CurrentPageSize,
                                                                  startDate = pgParms.CurrentStartDate,
                                                                  prevStartDate = pgParms.CurrentStartDate
                                                                }
                                                           )
                                            );
            }
            catch (Exception ex)
            {
                _statusMessage = "Error: "
                                 + ex.Message
                                 + "; "
                                 + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                _logger.LogError($"{_statusMessage}.");

                return await Task.FromResult(RedirectToPage("./EventsLog",
                                                            new
                                                            {
                                                                fromPOST = "Yes",
                                                                haveError = "Yes",
                                                                workFlowId = pgParms.WorkFlowId
                                                            }
                                                           )
                                            );
            }
        }

        protected void setEmptyPage(string workFlowId)
        {
            if (pgParms == null)
            {
                pgParms = new pageParameters();
            }
            pgParms.uid = currentUser.Id;
            pgParms.WorkFlowId = workFlowId ?? Guid.NewGuid().ToString() + "_a";
            pgParms.CurrentStartDate = "24";
            pgParms.CurrentSort = "sortA";
            pgParms.SortDirection = "_";
            pgParms.CurrentFilter = "";
            pgParms.CurrentPageSize = GlobalParameters.DefaultPageSize;
            PagingNotification = "";
            GeneralLogPage = new List<GeneralLog_v>();
            pgParms.TotalPages = 0;
            pgParms.PageIndex = 1;
            pgParms.CombinedFilter_updated = 0;
            pgParms.CombinedFilter_filter = GlobalParameters._combinedFiltersTemplates.EventsLog;
        }
    }
}