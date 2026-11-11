using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace RPA.MTSInspections.Controllers
{
    // Anonymous so a failed sign-in or a failed request can still show an error page; the exception handler
    // re-executes the original method, so the antiforgery check cannot apply here.
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public class ErrorController : Controller
    {
        [Route("Error")]
        public IActionResult Index()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

            return View("~/Views/Shared/Error.cshtml", exception);
        }
    }
}
