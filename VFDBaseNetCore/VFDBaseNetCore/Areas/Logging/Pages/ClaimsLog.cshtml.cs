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

using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;
using MTF.Utilities;
using MTF.Areas.CommonDB.Data;

using MTF.Areas.Logging.Data;
using MTF.Areas.Logging.Models;

using Microsoft.EntityFrameworkCore;

namespace MTF.Utilities
{
    
}

namespace MTF.Areas.Logging.Pages
{
    public class claimsInvariant
    {
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public string WorkFlowId { get; set; }
        public string SortOrder { get; set; }
        public string SortDirection { get; set; }
        public string SearchField { get; set; }
        public string SearchFilter { get; set; }
        public string StartDate { get; set; }
        public claimsInvariant()
        {
            StartDate = "24";
            SortOrder = "sortA";
            SortDirection = "_";
            SearchFilter = "";
            PageSize = GlobalParameters.DefaultPageSize;
            TotalPages = 0;
            PageIndex = 1;
            SearchField = "author";
        }
    }
    [Authorize]
    public class ClaimsLogModel : mtfPageModel<claimsInvariant, ClaimsLogModel>
    {
        private LogDB_Context _LogDB_Context { get; set; }
        public List<SelectListItem> AvailableStartDates { get; set; }
        public List<SelectListItem> AvailableSearchFields { get; set; }

        public ClaimsLogModel(CommonIdent context,
                              IAuthorizationService authorizationService,
                              UserManager<CommonUser> userManager,
                              ILogger<ClaimsLogModel> logger,
                              IpSafeList ipSafeList,
                              LogDB_Context LogDB_Context
                             )
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Claims Log"
                  )
        {
            _LogDB_Context = LogDB_Context;
            
            AvailableStartDates =
                new List<SelectListItem>(new[] { new SelectListItem { Value = "1", Text = "Last Hour"},
                                                 new SelectListItem { Value = "24", Text = "Last 24 Hours"},
                                                 new SelectListItem { Value = "168", Text = "Last Week"},
                                                 new SelectListItem { Value = "720", Text = "Last 30 Days"},
                                                 new SelectListItem { Value = "8760", Text = "Last Year"},
                                               }
                                        );
            AvailableSearchFields =
                new List<SelectListItem>(new[] { new SelectListItem { Value = "author", Text = "author"},
                                                 new SelectListItem { Value = "text", Text = "text"}
                                               }
                                        );
        }

        [BindProperty]
        public post_op _operation { get; set; }

        public List<ClaimsLog_v> ClaimsLogPage { get; set; }
        public string PagingNotification { get; set; }
        public CommonUser currentUser { get; set; }

        public async Task<IActionResult> OnGetAsync(string haveError,
                                                    string fromPOST,
                                                    string workFlowId,
                                                    string startDate,
                                                    string SortOrder,
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

                // workflow processing
                await _prologue(workFlowId == null);
                _invariants.WorkFlowId = workFlowId ?? Guid.NewGuid().ToString() + "a"; ;

                if (haveError == "Yes")
                {
                    setEmptyPage(workFlowId);

                    await _epilogue();
                    return await Task.FromResult(RedirectToPage("./ClaimsLog",
                                                                new
                                                                {
                                                                    haveError = "Proceeded",
                                                                    workFlowId = _invariants.WorkFlowId
                                                                }
                                                               )
                                                );
                }

                // Start date choice
                bool startDateChanged = false;
                if (startDate != null && startDate != _invariants.StartDate)
                {
                    _invariants.StartDate = startDate;
                    startDateChanged = true;
                }

                // Work with sorting by fields
                if (SortOrder != null && SortOrder != _invariants.SortOrder)
                {
                    _invariants.SortOrder = SortOrder;
                    _invariants.SortDirection = "_";
                }
                else if (SortOrder != null && SortOrder == _invariants.SortOrder)
                {
                    _invariants.SortDirection = _invariants.SortDirection == "_desc" ? "_" : "_desc";
                }
                
                // Work with search string
                if (searchField != null && searchField != _invariants.SearchField)
                {
                    _invariants.SearchField = searchField;
                    _invariants.SearchFilter = "";
                    searchString = "";
                }
                if (searchString != null && searchString != _invariants.SearchFilter)
                {
                    searchString = (searchString == "\u0002\u0003") ? "" : searchString;
                    _invariants.SearchFilter = searchString;
                }

                if (pageSize != null && pageSize != _invariants.PageSize)
                {
                    _invariants.PageSize = (int)pageSize ;
                }

                // Work with paginated data
                var _ps = _invariants.PageSize;
                var _pi = pageIndex ?? 1;
                var recStart = (_pi - 1) * _ps + 1;
                var recEnd = _pi * _ps;

                // do we need to build a new cache for current user?
                if (
                        workFlowId == null ||
                        startDateChanged
                    )
                {
                    // Building identities cache for current user
                    var rc01 = (await _LogDB_Context.getScalarValue("call claimsLogCache_Drop",
                                                                                            currentUser.Id,
                                                                                            _invariants.WorkFlowId
                                                                                            ));
                    var rc02 = (await _LogDB_Context.getScalarValue("call claimsLogCache_Build",
                                                                                            currentUser.Id,
                                                                                            _invariants.WorkFlowId,
                                                                                            _invariants.StartDate
                                                                                            ));
                }

                // Get the page of Data itself
                
                ClaimsLogPage = (await _LogDB_Context.claimsLog_v.FromSqlRaw("call claimsLogCache_GetPage_wcmpflt ({0},{1},{2},{3}, {4},{5}, {6});",
                                                                             currentUser.Id,
                                                                             String.IsNullOrEmpty(_invariants.SearchField) ? "%" : _invariants.SearchFilter,
                                                                             _invariants.SortOrder,
                                                                             _invariants.SortDirection,
                                                                             _pi, _ps,
                                                                             String.IsNullOrEmpty(_invariants.SearchField) ? "author" : _invariants.SearchField)
                                                                 .AsNoTracking()
                                                                 .ToListAsync());
                // Find total number of records
                var count = ClaimsLogPage.Count>0 ? ClaimsLogPage.First().numRecs : 0;
                
                recEnd = recEnd > count ? count : recEnd;
                PagingNotification = "recs: " + recStart.ToString() + "-" + recEnd.ToString() + " of " + count.ToString();

                _invariants.PageIndex = _pi;
                var tp = (int)Math.Ceiling(count / (double)_ps);
                _invariants.TotalPages = tp <= 0 ? 1 : tp;

                if (_pi < 1 || _pi > _invariants.TotalPages)
                {
                    throw new Exception("Page number out of possible range.") ;
                }

                await _epilogue();

                return await Task.FromResult(Page());
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);

                setEmptyPage(workFlowId);
                
                return await Task.FromResult(RedirectToPage("./ClaimsLog",
                                                            new { haveError = "Yes",
                                                                  workFlowId = _invariants.WorkFlowId
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

                await _prologue();

                DbContextWithScalarReturn.ScalarReturn rc;
                switch (_operation._op)
                {
                    case "selectAllVisible":
                        rc = await _LogDB_Context.getScalarValue("call claimsLogCache_selectAllVisible",
                                                                 currentUser.Id,
                                                                 String.IsNullOrEmpty(_invariants.SearchField) ? "%" : _invariants.SearchFilter,
                                                                 String.IsNullOrEmpty(_invariants.SearchField) ? "author" : _invariants.SearchField);
                        break;
                    case "invertSelection":
                        rc = await _LogDB_Context.getScalarValue("call claimsLogCache_invertSelectionAllVisible",
                                                                 currentUser.Id,
                                                                 String.IsNullOrEmpty(_invariants.SearchField) ? "%" : _invariants.SearchFilter,
                                                                 String.IsNullOrEmpty(_invariants.SearchField) ? "author" : _invariants.SearchField);
                        break;
                    case "deselectAllVisible":
                        rc = await _LogDB_Context.getScalarValue("call claimsLogCache_deSelectAllVisible",
                                                                 currentUser.Id,
                                                                 String.IsNullOrEmpty(_invariants.SearchField) ? "%" : _invariants.SearchFilter,
                                                                 String.IsNullOrEmpty(_invariants.SearchField) ? "author" : _invariants.SearchField);
                        break;
                    default:
                        throw new Exception("Team View - Unknown operation.");
                };

                await _epilogue();
                return await Task.FromResult(RedirectToPage("./ClaimsLog",
                                                            new { fromPOST = "Yes",
                                                                  workFlowId = _invariants.WorkFlowId
                                                                }
                                                           )
                                            );
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);

                return await Task.FromResult(RedirectToPage("./ClaimsLog",
                                                            new
                                                            {
                                                                fromPOST = "Yes",
                                                                haveError = "Yes",
                                                                workFlowId = _invariants.WorkFlowId
                                                            }
                                                           )
                                            );
            }
        }

        protected void setEmptyPage(string workFlowId)
        {
            PagingNotification = "";
            ClaimsLogPage = new List<ClaimsLog_v>();

            _invariants.WorkFlowId = workFlowId ?? Guid.NewGuid().ToString() + "_a";
            
        }

        public async Task<IActionResult> OnGetSelectionToggleAsync(int? id)
        {
            try
            {
                await _prologue();

                if (id == null)
                {
                    throw new FormatException("Unknown cache entity id.");
                }

                var rc01 = await _LogDB_Context.getScalarValue("call claimsLogCache_SelectionToggle",
                                                                id
                                                               );

                await _epilogue();

                return await Task.FromResult(Partial("/Pages/Shared/_dummy.cshtml","OK"));
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);

                return await Task.FromResult(Partial("/Pages/Shared/_dummy.cshtml",_statusMessage));
            }
        }
    }
}