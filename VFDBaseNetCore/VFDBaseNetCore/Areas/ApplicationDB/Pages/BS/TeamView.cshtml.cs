/* https://www.mikesdotnetting.com/article/350/razor-pages-and-bootstrap-lazy-loading-tabs */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.UI.Services;

using Microsoft.EntityFrameworkCore;

using MTF.Areas.Identity.Data;
using MTF.Areas.ApplicationDB.Data;
using MTF.Utilities;
using MTF.Areas.ApplicationDB.Pages.BS.Models;

namespace MTF.Areas.ApplicationDB.BS
{
    public class tvInvariants
    {
        public string searchString { get; set; }
    }
    [Authorize]
    public class TeamViewModel : applicationPageModel<tvInvariants, TeamViewModel>
    {
        private readonly IEmailSender _emailSender;
        public List<SelectListItem> AvailableUserRoles { get; set; }
        public TeamViewModel(CommonIdent context,
                                 IAuthorizationService authorizationService,
                                 UserManager<CommonUser> userManager,
                                 AppDB_Context appDBContext,
                                 ILogger<TeamViewModel> logger,
                                 IpSafeList ipSafeList,
                                 IEmailSender emailSender
                                )
            : base(context,
                   authorizationService,
                   userManager,
                   appDBContext,
                   logger,
                   ipSafeList,
                   "About " + GlobalParameters.TeamNameAlias
                  )
        {
            AvailableUserRoles =
                new List<SelectListItem>(new[] { new SelectListItem { Value = "0", Text = "ordinal"},
                                                 new SelectListItem { Value = "1", Text = "manager"},
                                                 new SelectListItem { Value = "2", Text = "banned"},
                                               }
                                        );
            _emailSender = emailSender;
        }

        [BindProperty]
        public post_op _operation { get; set; }

        //[BindProperty(SupportsGet = true)]
        public bs_teams_v _team { get; set; }

        public List<bs_team_users_v> _teamUsers { get; set; }
        public List<bs_invite_traps_v> _teamInvitations { get; set; }
        public List<bs_invite_traps_v> _teamHistory { get; set; }
        public List<bs_marks_v> _teamMarks { get; set; }
        public List<bs_team_configs_v> _teamConfigs { get; set; }

        public async Task<IActionResult> OnGetAsync(string haveError,
                                                    string fromPOST,
                                                    int id)
        {
            try
            {
                if (haveError == "Yes" && fromPOST != "Yes")
                {
                    _team = new bs_teams_v();
                    _teamUsers = new List<bs_team_users_v>();
                    return await Task.FromResult(Page());
                }
                await _prologue();

                _team = (await appdbcnt._bs_teams_v.FromSqlRaw("call bt_teams_v_get_byid ({0});",
                                                               id)
                                                   .AsNoTracking()
                                                   .ToListAsync()).FirstOrDefault();
                _teamUsers = (await appdbcnt._bs_team_users_v.FromSqlRaw("call bt_team_users_get_byid ({0},{1});",
                                                                         id,
                                                                         "")
                                                                .AsNoTracking()
                                                                .ToListAsync());


                await _epilogue();
                return await Task.FromResult(Page());
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                return await Task.FromResult(RedirectToPage("./TeamView",
                                                            new { haveError = "Yes",
                                                                  id = id}
                                                           )
                                            );
            }          
        }

        public async Task<IActionResult> OnGetInvitationsAsync(int? id)
        {
            try
            {
                if (id == null)
                {
                    return BadRequest(); ;
                }
                await _prologue();

                _teamInvitations = (await appdbcnt._bs_invite_traps_v.FromSqlRaw("call bt_team_invitations_get ({0},{1});",
                                                                                 id,
                                                                                 "")
                                                                     .AsNoTracking()
                                                                     .ToListAsync());

                await _epilogue();
                return await Task.FromResult(Partial("./_TeamViewInvitations", _teamInvitations));
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                return await Task.FromResult(RedirectToPage("./TeamView",
                                                            new
                                                            {
                                                                haveError = "Yes",
                                                                id = id
                                                            }
                                                           )
                                            );
            }
        }
        public async Task<IActionResult> OnGetHistoryAsync(string haveError,
                                                           string fromPOST,
                                                           int id)
        {
            try
            {
                if (haveError == "Yes" && fromPOST != "Yes")
                {
                    _team = new bs_teams_v();
                    return await Task.FromResult(Page());
                }
                await _prologue();

                _teamHistory = (await appdbcnt._bs_invite_traps_v.FromSqlRaw("call bt_team_history_get ({0},{1});",
                                                                             id,
                                                                             "")
                                                                 .AsNoTracking()
                                                                 .ToListAsync());

                await _epilogue();
                return await Task.FromResult(Partial("./_TeamViewHistory", _teamHistory));
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                var sm = _statusMessage;
                _statusMessage = null;

                return await Task.FromResult(Partial("/Pages/Shared/_dummy.cshtml", sm));
            }
        }
        public async Task<IActionResult> OnGetMarksAsync(string haveError,
                                                         string fromPOST,
                                                         int id)
        {
            try
            {
                if (haveError == "Yes" && fromPOST != "Yes")
                {
                    _team = new bs_teams_v();
                    return await Task.FromResult(Page());
                }
                await _prologue();

                _teamMarks = (await appdbcnt._bs_marks_v.FromSqlRaw("call bm_marks_v_get ({0},{1});",
                                                                             id,
                                                                             "")
                                                                 .AsNoTracking()
                                                                 .ToListAsync());

                await _epilogue();
                return await Task.FromResult(Partial("./_TeamViewMarks", _teamMarks));
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                var sm = _statusMessage;
                _statusMessage = null;

                return await Task.FromResult(Partial("/Pages/Shared/_dummy.cshtml", sm));
            }
        }

        public async Task<IActionResult> OnGetConfigsAsync(int? id)
        {
            try
            {
                if (id == null)
                {
                    return BadRequest(); ;
                }
                await _prologue();

                _teamConfigs = (await appdbcnt._bs_team_configs_v.FromSqlRaw("call bs_team_configs_list ({0});",
                                                                                 id)
                                                                     .AsNoTracking()
                                                                     .ToListAsync());

                await _epilogue();
                return await Task.FromResult(Partial("./_TeamViewConfigs", _teamConfigs));
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                return await Task.FromResult(RedirectToPage("./TeamView",
                                                            new
                                                            {
                                                                haveError = "Yes",
                                                                id = id
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

                DbContextWithScalarReturn.ScalarReturn rc;
                switch (_operation._op)
                {
                    case "submitRole":
                        rc = await appdbcnt.getScalarValue("call bs_team_member_change_role", _operation._prm, _operation._prm2, _operation._src);
                        break;
                    case "excludeMember":
                        rc = await appdbcnt.getScalarValue("call bs_team_member_exclude", _operation._prm, _operation._src);
                        break;
                    case "addInvitation":
                        rc = await appdbcnt.getScalarValue("call bs_team_invitation_add", _operation._prm, _operation._src);
                        if (rc.RetValueInt == rc.RetValueInt)// 0)
                        { // should send e-mail
                            var callbackUrl = Url.Page(
                            "/Account/Login",
                            pageHandler: null,
                            values: new { area = "Identity" },
                            protocol: Request.Scheme);

                            Task fict = _emailSender.SendEmailAsync(rc.RetValueString, "You are invited to sollers's Portals",
                            $"<p>Please be informed that you are invited to one of a "
                            +MTF.GlobalParameters.TeamsNameAlias
                            +$" on sollers's Portals.</p><p></p><p>You can log in or register as a new user by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.</p>"
                            +$"{_operation._prm2}");
                        };
                        _logger.LogInformation($"Invitation has been sent to {rc.RetValueString} by {_userRef.FullName}");
                        break;
                    case "cancelInvite":
                        rc = await appdbcnt.getScalarValue("call bs_team_invitation_cancel", _operation._prm, _operation._src);
                        break;
                    case "addMark":
                        rc = await appdbcnt.getScalarValue("call bm_mark_add", _operation._prm, _operation._src);
                        break;
                    case "excludeMark":
                        rc = await appdbcnt.getScalarValue("call bm_mark_exclude", _operation._prm, _operation._src);
                        break;
                    case "updateConfigValue":
                        rc = await appdbcnt.getScalarValue("call bs_team_config_set", _operation._src, _operation._prm, _operation._prm2);
                        break;
                    default:
                        throw new Exception("Team View - Unknown operation.");
                };

                await _epilogue();
                return await Task.FromResult(RedirectToPage("./TeamView",
                                                            new { fromPOST = "Yes",
                                                                  id = _operation._src}
                                                           )
                                            );
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                return await Task.FromResult(RedirectToPage("./TeamView",
                                                            new
                                                            {
                                                                fromPOST = "Yes",
                                                                haveError = "Yes",
                                                                id = _operation._src
                                                            }
                                                           )
                                            );
            }
        }
    }
}