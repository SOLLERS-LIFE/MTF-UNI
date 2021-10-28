using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity.UI.Services;

using Microsoft.Extensions.Logging;

using MTF;
using MTF.Utilities;
using MTF.Areas.CommonDB.Data;
using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;
using MTF.Areas.Identity.Pages.SURoom.Models;
using MTF.Areas.Identity.Services.UsersActivity;

namespace MTF.Areas.Identity.Pages.SURoom
{
    [Authorize]
    public class CommonUsersListModel : mtfPageModel<pageParameters, CommonUsersListModel>
    {
        public string PagingNotification { get; set; }

        private UAStore _UAStore { get; set; }
        public string _announcement { get; set; }

        protected readonly IEmailSender _emailSender;

        public CommonUsersListModel(
                                    CommonIdent context,
                                    IAuthorizationService authorizationService,
                                    UserManager<CommonUser> userManager,
                                    RoleManager<CommonRole> roleManager,
                                    CommonDB_Context commonDB_Context,
                                    ILogger<CommonUsersListModel> logger,
                                    IpSafeList ipSafeList,
                                    IEmailSender emailSender
                                   )
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Users List"
                  )
        {
            _roleManager = roleManager;
            _commonDB_Context = commonDB_Context;
            _UAStore = GlobalParameters._UAStore;
            _emailSender = emailSender;

            AvailableRoles = _roleManager.Roles.Select(a =>
                            new SelectListItem
                            {
                                Value = a.Name,
                                Text = a.Name
                            }).ToList();
            AvailableRoles.Add(new SelectListItem { Value = "--ALL USERS--", Text="--ALL USERS--"});
            AvailableRoles.Add(new SelectListItem { Value = "--LOGGED IN USERS--", Text = "--LOGGED IN USERS--" });

            AvailableSearchFields = 
                new List<SelectListItem>(new[] { new SelectListItem { Value = "by EMail", Text = "by EMail"},
                                                 new SelectListItem { Value = "by Name", Text = "by Name"}
                                               }
                                        );
        }
        // To provide roles list and role for choice
        public RoleManager<CommonRole> _roleManager { get; set; }
        public List<SelectListItem> AvailableRoles { get; set; }
        // to provide search field choice
        public List<SelectListItem> AvailableSearchFields { get; set; }
        public CommonDB_Context _commonDB_Context { get; set; }
        public List<cachedCommonUser> CommonUsersPage { get; set; }
        public CommonUser currentUser { get; set; }

        [BindProperty]
        public post_op op { get; set; }

        public async Task<IActionResult> OnGetAsync(
                                                     string haveError,
                                                     string workFlowId,
                                                     string searchRole,
                                                     string prevSearchRole,
                                                     string newSortOrder,
                                                     string prevSortOrder,
                                                     string prevSortDirection,
                                                     string searchString,
                                                     string searchField,
                                                     int? pageIndex,
                                                     int? pageSize
                                                    )
        {
            try
            {
                await _prologue(workFlowId == null);

                currentUser = await verifySUwr();
                if (currentUser == null || !IsSourceIPSafe())
                {
                    await _epilogue();
                    return NotFound();
                }

                if (haveError == "Yes")
                {
                    setEmptyPage(workFlowId);
                    await _epilogue();

                    return await Task.FromResult(Page());
                }

                _announcement = await getLatestAnnouncement();

                _invariants.WorkFlowId = workFlowId ?? Guid.NewGuid().ToString() + "a";
                _invariants.CurrentRole = searchRole ?? "--ALL USERS--";

                if (
                     workFlowId == null ||
                     searchRole == null ||
                     searchRole != prevSearchRole ||
                     searchRole == "--LOGGED IN USERS--"
                   )
                {
                    // Building identities cache for current user
                    var a = await _commonDB_Context.getScalarValue("call commonUsersCache_Drop",
                                                               currentUser.Id,
                                                               _invariants.WorkFlowId
                                                              );
                    List<CommonUser> lu;
                    switch (_invariants.CurrentRole)
                    {
                        case "--ALL USERS--":
                            lu = _userManager.Users.ToList(); // await _userManager.FindByIdAsync(el)
                            break;
                        case "--LOGGED IN USERS--":
                            lu = new List<CommonUser>();
                            List<string> auIds = await _UAStore.GetActualEntries();
                           
                            foreach (string id in auIds)
                            {
                                lu.Add(await _userManager.FindByIdAsync(id));
                            }
                            break;
                        default:
                            lu = (await _userManager.GetUsersInRoleAsync(_invariants.CurrentRole)).ToList();
                            break;
                    }
                    foreach (var cu in lu)
                    {
                        await _commonDB_Context.cachedCommonUsers.AddAsync(new cachedCommonUser(cu, currentUser.Id));
                    }
                    await _commonDB_Context.SaveChangesAsync();
                }

                // Sort field choice
                _invariants.CurrentSearchField = searchField ?? "by EMail";

                // Work with sorting by fields
                if (newSortOrder == null)
                {
                    newSortOrder = "sortA";
                    prevSortOrder = "sortA";
                    _invariants.SortDirection = "_";
                }
                if (newSortOrder != prevSortOrder)
                {
                    _invariants.SortDirection = "_";
                }
                else
                {
                    prevSortDirection ??= "_desc";
                    _invariants.SortDirection = prevSortDirection == "_desc" ? "_" : "_desc";
                }
                _invariants.CurrentSort = newSortOrder;

                // Work with search string
                _invariants.CurrentFilter = searchString;
                
                // Work with paginated data
                var _ps = pageSize ?? _invariants.CurrentPageSize;
                _invariants.CurrentPageSize = _ps;
                var _pi = pageIndex ?? 1;
                var recStart = (_pi - 1) * _ps + 1;
                var recEnd = _pi * _ps;

                // Get the page of Data itself
                CommonUsersPage = await _commonDB_Context.cachedCommonUsers
                                          .FromSqlRaw("call commonUsersCache_wSrtAndFlt_Data ({0},{1},{2},{3},{4}, {5},{6});",
                                                      currentUser.Id,
                                                      _invariants.CurrentSearchField,
                                                      String.IsNullOrEmpty(_invariants.CurrentFilter) ? "%" : _invariants.CurrentFilter,
                                                      _invariants.CurrentSort,
                                                      _invariants.SortDirection,
                                                      _pi, _ps)
                                          .AsNoTracking()
                                          .ToListAsync();
                // Find total number of records
                var count = (await _commonDB_Context.getScalarValue("call commonUsersCache_wSrtAndFlt_Count",
                                                             currentUser.Id,
                                                             _invariants.CurrentSearchField,
                                                             String.IsNullOrEmpty(_invariants.CurrentFilter) ? "%" : _invariants.CurrentFilter,
                                                             _invariants.WorkFlowId
                                                            )
                            ).RetValueInt;

                recEnd = recEnd > count ? count : recEnd;
                PagingNotification = "recs: " + recStart.ToString() + "-" + recEnd.ToString() + " of " + count.ToString();

                _invariants.PageIndex = _pi;
                var tp = (int)Math.Ceiling(count / (double)_ps);
                _invariants.TotalPages = tp <= 0 ? 1 : tp;

                if (_pi < 1 || _pi > _invariants.TotalPages)
                {
                    throw new Exception("Page number out of possible range.");
                }

                await _epilogue();

                return await Task.FromResult(Page());
            }
            catch (Exception ex)
            {
                setEmptyPage(workFlowId);

                await _epilogue_catcher(ex);

                return await Task.FromResult(RedirectToPage("./CommonUsersList",
                                                            new
                                                            {
                                                                haveError = "Yes",
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
                await _prologue();

                currentUser = await verifySUwr();
                if (currentUser == null || !IsSourceIPSafe())
                {
                    await _epilogue();
                    return NotFound();
                }

                string invSortDirection = _invariants.SortDirection ?? "_desc";
                invSortDirection = invSortDirection == "_desc" ? "_" : "_desc";

                switch (op._op)
                {
                    case "_disconnectAllButSUS":
                        await disconnectAllActiveButSUS();
                        break;
                    case "_CloseSystemButSUS":
                        await addLoginsdisabler(op._prm);
                        await disconnectAllActiveButSUS();
                        break;
                    case "_ChangeMessage":
                        if (!String.IsNullOrEmpty(op._prm))
                        {
                            await addLoginsdisabler(op._prm);
                        }
                        break;
                    case "_OpenSystem":
                        await addLoginsdisabler("");
                        break;
                    case "_ChangeAnnouncement":
                        await addAnnouncement(op._prm);
                        break;
                    case "_disableAnnouncement":
                        await addAnnouncement("");
                        break;
                    case "_emailSend":
                        await sendEmails(op._prm);
                        break;
                    default:
                        throw new Exception("CommonUsersList - Unknown operation.");
                }

                await _epilogue();
                return await Task.FromResult(
                        RedirectToPage("./CommonUsersList", new
                        {
                            workFlowId = _invariants.WorkFlowId,
                            newSortOrder = _invariants.CurrentSort,
                            prevSortOrder = _invariants.CurrentSort,
                            prevSortDirection = invSortDirection,
                            searchString = _invariants.CurrentFilter,
                            pageIndex = _invariants.PageIndex,
                            pageSize = _invariants.CurrentPageSize,
                            searchRole = _invariants.CurrentRole,
                            prevSearchRole = _invariants.CurrentRole,
                            searchField = _invariants.CurrentSearchField
                        }
                                     ));
            } catch (Exception ex)
            {
                await _epilogue_catcher(ex);

                return await Task.FromResult(RedirectToPage("./CommonUsersList",
                                                            new
                                                            {
                                                                fromPOST = "Yes",
                                                                haveError = "Yes",
                                                                workFlowId = _invariants.WorkFlowId,
                                                                newSortOrder = _invariants.CurrentSort,
                                                                prevSortOrder = _invariants.CurrentSort,
                                                                searchString = _invariants.CurrentFilter,
                                                                pageIndex = _invariants.PageIndex,
                                                                pageSize = _invariants.CurrentPageSize,
                                                                searchRole = _invariants.CurrentRole,
                                                                prevSearchRole = _invariants.CurrentRole,
                                                                searchField = _invariants.CurrentSearchField
                                                            }
                                                           )
                                            );
            }
        }

        protected void setEmptyPage(string workFlowId)
        {
            _invariants.uid = currentUser.Id;
            _invariants.WorkFlowId = workFlowId ?? Guid.NewGuid().ToString() + "_a";
            _invariants.CurrentRole = "--ALL USERS--";
            _invariants.CurrentSearchField = "by EMail";
            _invariants.CurrentSort = "sortA";
            _invariants.SortDirection = "_";
            _invariants.CurrentFilter = "";
            _invariants.CurrentPageSize = GlobalParameters.DefaultPageSize;
            PagingNotification = "";
            CommonUsersPage = new List<cachedCommonUser>();
            _invariants.TotalPages = 0;
            _invariants.PageIndex = 1;
        }

        protected async Task<int> disconnectAllActiveButSUS()
        {
            List<string> auIds = await _UAStore.GetActualEntries();

            int cnt = 0;
            foreach (string id in auIds)
            {
                var usr = await _userManager.FindByIdAsync(id);
                if (!await IsSU(usr))
                {
                    await _userManager.UpdateSecurityStampAsync(usr); // in conjaction with services.Configure<SecurityStampValidatorOptions>
                    await _UAStore.EntryComprimizedHard(id);

                    ++cnt;
                }
            }

            return cnt;
        }

        protected async Task<string> getLatestAnnouncement ()
        {
            var ann = (await _commonDB_Context._announcements.FromSqlRaw("call cdb.sa_get_latest_announcement;")
                                                            .AsNoTracking()
                                                            .ToListAsync())
                                                            .FirstOrDefault();
            if (ann == null)
            { GlobalParameters._announcement = ""; }
            else
            {
                GlobalParameters._announcement = ann.content;
                GlobalParameters._annDate = ann.DateIn;
            }

            return GlobalParameters._announcement;
        }

        protected async Task<int> addAnnouncement(string cont)
        {
            cont = cont == null ? "" : cont;
            var rc = await _commonDB_Context.getScalarValue("call cdb.sa_add_announcement",
                                                             cont
                                                           );
            await getLatestAnnouncement();

            return rc.RetValueInt;
        }

        protected async Task<int> addLoginsdisabler(string cont)
        {
            cont = cont == null ? "" : cont;
            var rc = await _commonDB_Context.getScalarValue("call cdb.sa_add_loginsdisabler",
                                                             cont
                                                           );
            await getLatestLoginsdisabler();

            return rc.RetValueInt;
        }

        protected async Task<string> getLatestLoginsdisabler()
        {
            var ann = (await _commonDB_Context._loginsdisablers.FromSqlRaw("call cdb.sa_get_latest_loginsdisabler;")
                                                            .AsNoTracking()
                                                            .ToListAsync())
                                                            .FirstOrDefault();
            if (ann == null)
            { GlobalParameters._systemClosedButSUS = ""; }
            else
            {
                GlobalParameters._systemClosedButSUS = ann.content;
            }

            return GlobalParameters._systemClosedButSUS;
        }

        protected async Task<int> sendEmails(string msg)
        {
            CommonUsersPage = await _commonDB_Context.cachedCommonUsers
                                          .FromSqlRaw("call commonUsersCache_wSrtAndFlt_Data ({0},{1},{2},{3},{4}, {5},{6});",
                                                      currentUser.Id,
                                                      _invariants.CurrentSearchField,
                                                      String.IsNullOrEmpty(_invariants.CurrentFilter) ? "%" : _invariants.CurrentFilter,
                                                      "No",
                                                      "No",
                                                      0, 0)
                                          .AsNoTracking()
                                          .ToListAsync();

            int cnt = 0;
            foreach (var usr in CommonUsersPage)
            {
                Task fict = _emailSender.SendEmailAsync(usr.Email,
                                                        "sollers's Portals Administrator Note",
                                                        $"<p>Hi {usr.toBeAddressed},</p><p></p>{msg}"
                                                       );
                ++cnt;
            }

            return cnt;
        }
    }
}
