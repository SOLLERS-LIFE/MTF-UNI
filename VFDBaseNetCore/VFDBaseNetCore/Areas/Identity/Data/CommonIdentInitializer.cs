using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using MTF.Areas.CommonDB.Models;
using MTF.Areas.CommonDB.Data;

namespace MTF.Areas.Identity.Data
{
    public static class CommonIdentInitializer
    {
        public static async Task<int> DoIt
                                 (IConfiguration config,
                                  RoleManager<CommonRole> roleManager,
                                  UserManager<CommonUser> userManager,
                                  IServiceProvider services
                                 )
        {
            var CDB = services.GetService<CommonIdent>();
            if (!CDB.configCommon.Any())
            {
                CDB.configCommon.Add(new CommonConfig
                    {
                        VersionOfSOLE = config.GetSection("MTF").GetValue<string>("Version"),
                        DeployDate = config.GetSection("MTF").GetValue<DateTime>("DeployDate"),
                        UpdateDate = config.GetSection("MTF").GetValue<DateTime>("UpdateDate")
                }
                );
            }
            await CDB.SaveChangesAsync();

            var cnf = await CDB.configCommon.FindAsync(1);

            cnf.VersionOfSOLE = config.GetSection("MTF").GetValue<string>("Version");
            cnf.DeployDate = config.GetSection("MTF").GetValue<DateTime>("DeployDate");
            cnf.UpdateDate = config.GetSection("MTF").GetValue<DateTime>("UpdateDate");

            string sus_Id, everyone_Id, testers_Id;
            if (
                (cnf.SUSRID == null) ||
                ((await roleManager.FindByIdAsync(cnf.SUSRID)) == null)
               )
            {
                sus_Id = await EnsureRole(roleManager, "SUS", RoleRights.su);
                cnf.SUSRID = sus_Id;
            }
            else
            {   
                sus_Id = cnf.SUSRID;
            }
            if (
                (cnf.EORID == null) ||
                ((await roleManager.FindByIdAsync(cnf.EORID)) == null)
               )
            {
                everyone_Id = await EnsureRole(roleManager, "EVERYONE", RoleRights.nothing);
                cnf.EORID = everyone_Id;
            }
            else
            {
                everyone_Id = cnf.EORID;
            }
            if (
                (cnf.TESTERSRID == null) ||
                ((await roleManager.FindByIdAsync(cnf.TESTERSRID)) == null)
               )
            {
                testers_Id = await EnsureRole(roleManager, "TESTERS", RoleRights.nothing);
                cnf.TESTERSRID = testers_Id;
            }
            else
            {
                testers_Id = cnf.TESTERSRID;
            }

            CommonUser su;
            if (
                (cnf.SUUID == null) ||
                ((await userManager.FindByIdAsync(cnf.SUUID)) == null)
               )
            {
                su = await EnsureSU(userManager,
                                    roleManager,
                                    cnf,
                                    config.GetSection("BusinessSecurity").GetValue<string>("su_name"),
                                    config.GetSection("BusinessSecurity").GetValue<string>("su_pwd")
                                    );
                cnf.SUUID = su.Id;
            }
            else
            {
                su = await userManager.FindByIdAsync(cnf.SUUID);
            }


            CDB.configCommon.Update(cnf);
            CDB.SaveChanges();

            GlobalParameters._everyone_Id = everyone_Id;
            GlobalParameters._everyone_Name = (await roleManager.FindByIdAsync(everyone_Id)).Name;
            GlobalParameters._sus_Id = sus_Id;
            GlobalParameters._sus_Name = (await roleManager.FindByIdAsync(sus_Id)).Name;
            GlobalParameters._testers_Id = testers_Id;
            GlobalParameters._testers_Name = (await roleManager.FindByIdAsync(testers_Id)).Name;

            return 0;
        }
        #region snippet_ensure
        public static async Task<string> EnsureRole (RoleManager<CommonRole> roleManager,
                                                     string role, Int32 rights = 0
                                                    )
        {
            CommonRole rl = await roleManager.FindByNameAsync(role);

            if (rl == null)
            {
                rl = new CommonRole(role, rights);
                var crs = await roleManager.CreateAsync(rl);
            }
           
            return rl.Id;
        }
        public static async Task<CommonUser> EnsureSU (UserManager<CommonUser> userManager,
                                                       RoleManager<CommonRole> roleManager,
                                                       CommonConfig cnf,
                                                       string name, string pswd
                                                      )
        {
            CommonUser user;
            user = await userManager.FindByNameAsync(name);
            if (user == null)
            {
                user = new CommonUser
                {
                    UserName = name,
                    Email = name,
                    FullName = name
                    , EmailConfirmed = true
                };
                var res = await userManager.CreateAsync(user, pswd);
                if (!res.Succeeded)
                {
                    user = null;
                }
            }

            if (user != null)
            {
                await userManager.AddToRoleAsync(user,(await roleManager.FindByIdAsync(cnf.EORID)).Name);
                await userManager.AddToRoleAsync(user,(await roleManager.FindByIdAsync(cnf.SUSRID)).Name);
            }
            
            return user;
        }
        #endregion
    }
}
