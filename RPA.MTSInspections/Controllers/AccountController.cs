using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RPA.MTSInspections.Controllers
{
    /// <summary>
    /// Entra sign-in and sign-out plus the anonymous status pages that must work before a user has a session or a role.
    /// </summary>
    public class AccountController : Controller
    {
        // GET: starts the Entra sign-in; only local return URLs are honoured.
        [AllowAnonymous]
        [HttpGet]
        public IActionResult SignIn(string returnUrl = null)
        {
            return Challenge(
                new AuthenticationProperties { RedirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : "/" },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        // GET: sign-out confirmation, so signing out is never triggered by a link or an image request.
        [Authorize]
        [HttpGet]
        [ActionName("SignOut")]
        public IActionResult SignOutConfirmation()
        {
            return View("SignOut");
        }

        // POST: antiforgery-protected sign-out of the application cookie and the Entra session.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("SignOut")]
        public IActionResult SignOutConfirmed()
        {
            return SignOut(
                new AuthenticationProperties { RedirectUri = Url.Action("SignedOut", "Account") },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult SignedOut()
        {
            return View();
        }

        // The user signed in successfully but holds none of the roles the page needs.
        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;

            return View();
        }

        // The Entra sign-in was rejected or failed; no claim values are shown or logged here.
        [AllowAnonymous]
        [HttpGet]
        public IActionResult AuthenticationFailed()
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;

            return View();
        }
    }
}
