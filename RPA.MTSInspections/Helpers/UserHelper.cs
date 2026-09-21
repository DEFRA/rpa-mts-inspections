using RPA.MTSInspections.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.MTSInspections.Helpers
{
    public class UserHelper
    {
        public static string CurrentUser()
        {
            string user = "Unknown";

            var context = HttpContext.Current;

            MTSInspectionsContext db = new MTSInspectionsContext();

            if (context != null)
            {
                user = context.User.Identity.Name.ToLower().Replace("earth\\", "").Replace("m0", "m");

                if (string.IsNullOrEmpty(user))
                {
                    user = "Unknown";
                }
            }

            return user;
        }

        public static string ExistingUser(string user)
        {
            var context = HttpContext.Current;

            MTSInspectionsContext db = new MTSInspectionsContext();

            if (context != null)
            {
                user = user.ToLower().Replace("earth\\", "").Replace("m0", "m");

                if (user == "" || user == null)
                {
                    user = "Unknown";
                }
            }
            else
            {
                user = "Unknown";
            }

            return user;
        }
    }
}