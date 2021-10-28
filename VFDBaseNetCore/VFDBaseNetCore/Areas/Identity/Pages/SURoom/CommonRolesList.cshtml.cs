using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;
using MTF.Utilities;
using MTF.Areas.CommonDB.Data;

namespace MTF.Areas.Identity.Pages.SURoom
{
    [Authorize]
    public class CommonRolesListModel : Ext_AuthPageModel
    {
        protected RoleManager<CommonRole> _rolesManager { get; set; }
        protected CommonDB_Context _commonDB { get; set; }

        public CommonRolesListModel (CommonIdent context,
                                 IAuthorizationService authorizationService,
                                 UserManager<CommonUser> userManager,
                                 ILogger<CommonRolesListModel> logger,
                                 RoleManager<CommonRole> rolesManager,
                                 CommonDB_Context commonDB,
                                 IpSafeList ipSafeList
                                )
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Roles List"
                  )
        {
            _rolesManager = rolesManager;
            _commonDB = commonDB;
        }
        
        // Idea to keep parameters for POST routine here
        public class InputParameters
        {
            [Required]
            [StringLength(50)]
            [Display(Name = "action name")]
            public string actionName { get; set; }
            [Required]
            [StringLength(50)]
            [Display(Name = "action parm")]
            public string actionParm { get; set; }
            [Required]
            [Display(Name = "action parm2")]
            public int actionParm2 { get; set; }

            public List<CommonRole> roles { get; set; }
        }
        [BindProperty]
        public InputParameters input { get; set; }

        public async Task<IActionResult> OnGetAsync(string haveError,
                                                    string fromPOST)
        {
            try
            {
                CommonUser currentUser = await verifySUwr();
                if (currentUser == null || !IsSourceIPSafe())
                {
                    return NotFound();
                }

                input = new InputParameters();
                input.roles = _rolesManager.Roles.OrderBy(s => s.Name).ToList();

                return await Task.FromResult(Page());
            }
            catch (Exception ex)
            {
                _statusMessage = "Error: "
                                 + ex.Message
                                 + "; "
                                 + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return await Task.FromResult(RedirectToPage("./CommonRolesList",
                                                            new { haveError = "Yes" }
                                                           )
                                            );
            }          
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                CommonUser currentUser = await verifySUwr();
                if (currentUser == null || !IsSourceIPSafe())
                {
                    return NotFound();
                }

                IdentityResult crs;
                switch (input.actionName)
                {
                    case "addRole":
                        if ((await _rolesManager.FindByNameAsync(input.actionParm)) == null)
                        {
                            crs = await _rolesManager.CreateAsync(new CommonRole(input.actionParm, 0));
                            if (!crs.Succeeded)
                            {
                                throw new Exception("CreateAsync error, to be qualified in future development");
                            }
                        }
                        else
                        {
                            throw new Exception($"The Role {@input.actionParm} already exists");
                        }
                        break;

                    case "renameRole":
                        var newName = input.roles[input.actionParm2].Name;
                        if ((await _rolesManager.FindByNameAsync(newName)) != null)
                        {
                            throw new Exception($"The Role with name {newName} already exists");
                        }
                        
                        CommonRole toUpd = await _rolesManager.FindByIdAsync(input.actionParm);
                        if (toUpd == null)
                        {
                            throw new Exception("Cannot find the role by Id, call your developers team");
                        }

                        crs = await _rolesManager.SetRoleNameAsync(toUpd, newName);
                        crs = await _rolesManager.UpdateAsync(toUpd);
                        if (!crs.Succeeded)
                        {
                            throw new Exception("SetRoleNameAsync error, to be qualified in future development");
                        }

                        break;

                    case "deleteRole":
                        var tr = await _rolesManager.FindByIdAsync(input.actionParm);
                        if (tr == null)
                        {
                            throw new Exception("The role doesnt exists");
                        }

                        var cnf = await _context.configCommon.FindAsync(1);
                        if (
                            (cnf.EORID == input.actionParm) ||
                            (cnf.SUSRID == input.actionParm) ||
                            (cnf.TESTERSRID == input.actionParm)
                            )
                        {
                            throw new Exception("This role cannot be deleted");
                        }

                        if ((await _userManager.GetUsersInRoleAsync(tr.Name)).Count>0)
                        {
                            throw new Exception("The role contains users - please exclude them first");
                        }

                        if ( !(await _rolesManager.DeleteAsync(tr)).Succeeded)
                        {
                            throw new Exception("DeleteAsync error, to be qualified in future development");
                        }

                        break;

                    default:
                        throw new Exception("Illegal Role Operation");
                }

                return await Task.FromResult(RedirectToPage("./CommonRolesList",
                                                            new { fromPOST = "Yes" }
                                                           )
                                            );
            }
            catch (Exception ex)
            {
                _statusMessage = "Error: "
                                 + ex.Message
                                 + "; "
                                 + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return await Task.FromResult(RedirectToPage("./CommonRolesList",
                                                            new { fromPOST = "Yes",
                                                                  haveError = "Yes" }
                                                           )
                                            );
            }
        }
    }
}