using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace RPA.MTSInspections.Tests
{
    // Gives a page model the context a Razor Pages host would supply (ViewData and TempData).
    public static class PageModelExtensions
    {
        public static T WithPageContext<T>(this T page) where T : PageModel
        {
            var http = new DefaultHttpContext();
            page.PageContext = new PageContext(new ActionContext(http, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.RazorPages.CompiledPageActionDescriptor()))
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            };
            page.TempData = new TempDataDictionary(http, Mock.Of<ITempDataProvider>());
            return page;
        }
    }
}
