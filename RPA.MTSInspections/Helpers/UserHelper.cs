namespace RPA.MTSInspections.Helpers
{
    public class UserHelper
    {
        private static IHttpContextAccessor httpContextAccessor;

        // The helper is used statically throughout the controllers and services, so the accessor is supplied once at start-up.
        public static void Configure(IHttpContextAccessor accessor)
        {
            httpContextAccessor = accessor;
        }

        public static string CurrentUser()
        {
            string user = "Unknown";

            var context = httpContextAccessor?.HttpContext;

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
            var context = httpContextAccessor?.HttpContext;

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
