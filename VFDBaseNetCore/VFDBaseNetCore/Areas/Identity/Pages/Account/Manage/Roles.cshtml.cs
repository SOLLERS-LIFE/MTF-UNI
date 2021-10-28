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

using MTF.Utilities;
using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;
using MTF.Areas.CommonDB.Data;

namespace MTF.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public partial class RolesModel : Ext_AuthPageModel
    {
        private readonly SignInManager<CommonUser> _signInManager;
        private readonly RoleManager<CommonRole> _rolesManager;
        private readonly CommonDB_Context _commonDB_Context;

        public RolesModel(
            CommonIdent context,
            IAuthorizationService authorizationService,
            RoleManager<CommonRole> rolesManager,
            UserManager<CommonUser> userManager,
            SignInManager<CommonUser> signInManager,
            CommonDB_Context commonDB_Context,
            ILogger<RolesModel> logger,
            IpSafeList ipSafeList)
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Roles")
        {
            _signInManager = signInManager;
            _rolesManager = rolesManager;
            _commonDB_Context = commonDB_Context;
            Input = new InputModel();
        }

        // Added to all predefined identity management models
        // to work with ANY account
        [BindProperty]
        public CommonUser _commonUser { get; set; }

        public List<CommonRole> rolesIncluded { get; set; }
        public List<CommonRole> rolesToInclude { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {
            [Required]
            [StringLength(50)]
            [Display(Name = "roleToActOn")]
            public string roleToActOn { get; set; }
            [Required]
            [StringLength(50)]
            [Display(Name = "roleIdToActOn")]
            public string roleIdToActOn { get; set; }
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
            id ??= (await _userManager.GetUserAsync(User)).Id;

            _commonUser = await verifySUunder(id);
            if (_commonUser == null)
            {
                return NotFound();
            }

            rolesIncluded = new List<CommonRole>();
            var rinc = await _userManager. GetRolesAsync(_commonUser);
            foreach (var el in rinc)
            { 
                rolesIncluded.Add(new CommonRole { Name = el, Id = (await _rolesManager.FindByNameAsync(el)).Id }) ;
            }

            rolesToInclude = _rolesManager.Roles.Where(s => !rolesIncluded.Any(p => p == s)).ToList();

            return Page();
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
                    case "include":
                        await _userManager.AddToRoleAsync(_commonUser, Input.roleToActOn);
                        break;

                    case "exclude":
                        var cnf = _context.configCommon.Find(1);

                        if (cnf.EORID == Input.roleIdToActOn)
                        {
                            throw (new Exception("User cannot be excluded from EVERYONE role"));
                        }
                        if (cnf.SUSRID == Input.roleIdToActOn &&
                            cnf.SUUID == Input.userId)
                        {
                            throw (new Exception("SU cannot be excluded from SUS role"));
                        }

                        await _userManager.RemoveFromRoleAsync(_commonUser, Input.roleToActOn);
                        break;

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

            return RedirectToPage("./Roles", new { id = _commonUser.Id });
        }
    }
}
