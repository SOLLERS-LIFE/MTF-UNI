using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;
using MTF.Areas.CommonDB.Data;

using MTF.Areas.ApplicationDB.Pages.BS.Models;
using MTF.Areas.ApplicationDB.Data;
using MTF.Utilities;

namespace MTF.Areas.Identity.Pages.Account.Manage
{
    public class teamsUsersInvariants
    {}

    [Authorize]
    public partial class TeamsUserModel : applicationPageModel<teamsUsersInvariants, TeamsUserModel>
    {
        public TeamsUserModel(
                                 CommonIdent context,
                                 IAuthorizationService authorizationService,
                                 UserManager<CommonUser> userManager,
                                 AppDB_Context appDBContext,
                                 IpSafeList ipSafeList,
                                 ILogger<TeamsUserModel> logger)
            : base(context,
                   authorizationService,
                   userManager,
                   appDBContext,
                   logger,
                   ipSafeList,
                   "TeamsUser")
        {
        }

        // Added to all predefined identity management models
        // to work with ANY account
        [BindProperty]
        public CommonUser _commonUser { get; set; }

        public List<bs_teams_v> teamsIncluded { get; set; }

        public List<bs_marks_v> MarksCanSee { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {
            [Required]
            [StringLength(50)]
            [Display(Name = "teamToActOn")]
            public string teamToActOn { get; set; }
            [Required]
            [StringLength(50)]
            [Display(Name = "teamIdToActOn")]
            public string teamIdToActOn { get; set; }
            [Required]
            [StringLength(50)]
            [Display(Name = "userId")]
            public string userId { get; set; }
            [Required]
            [StringLength(50)]
            [Display(Name = "action")]
            public string action { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            try
            {
                id ??= (await _userManager.GetUserAsync(User)).Id;
                _commonUser = await verifySUunder(id);
                if (_commonUser == null)
                {
                    return NotFound();
                }

                await _prologue();

                teamsIncluded = (await appdbcnt._bs_teams_v.FromSqlRaw("call bt_team_user_teams ({0});",
                                                                             id)
                                                                .AsNoTracking()
                                                                .ToListAsync());

                MarksCanSee = (await appdbcnt._bs_marks_v.FromSqlRaw("call bm_marks_user_marks ({0});",
                                                                           id)
                                                                .AsNoTracking()
                                                                .ToListAsync());

                await _epilogue();
                return Page();
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(string action)
        {
            _commonUser = await verifySUunder(Input.userId);
            if (_commonUser == null)
            {
                return NotFound();
            }
                        
            try
            {
                if (!ModelState.IsValid)
                {
                    throw (new Exception("Something went wrong with model validity"));
                }

                if (!await verifySU())
                {
                    throw (new Exception("You have not superuser rights"));
                }

                switch (Input.action)
                {
                    default:
                        throw (new Exception("Illegal operation Posted"));
                }

            }
            catch (Exception ex)
            {
                _statusMessage = "Error: "
                                + ex.Message
                                + "; "
                                + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
            }

            return RedirectToPage("./TeamsUser", new { id = _commonUser.Id });
        }
    }
}
