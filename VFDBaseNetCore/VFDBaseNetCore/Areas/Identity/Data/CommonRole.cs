using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
//using Pomelo.EntityFrameworkCore.MySql;

namespace MTF.Areas.Identity.Data
{
    // just an extention for standart identity role
    // the idea is to add superusers and ordinal users (all other groups)
    // I demonstrate here the technik to extend a identity role with
    // additional fields. Migrations works here!
    public class CommonRole : IdentityRole
    {
        public CommonRole ()
            : base ()
        { Rights = 0b0000_0000_0000_0000_0000_0000_0000_0000; }
        public CommonRole(string str)
            : base(str)
        { Rights = 0b0000_0000_0000_0000_0000_0000_0000_0000; }
        public CommonRole(string str,Int32 _rts)
            : base(str)
        { Rights = _rts; }

        public Int32 Rights { get; set; } 
    }

    public static class RoleRights
    {
        public static Int32 su = 0b0000_0001;
        public static Int32 nothing = 0b0000_0000;
    }
}
