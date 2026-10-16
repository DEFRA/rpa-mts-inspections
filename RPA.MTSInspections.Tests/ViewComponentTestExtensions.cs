using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RPA.MTSInspections.Tests
{
    public static class ViewComponentTestExtensions
    {
        // Gives a view component the view data it normally receives from MVC so View(model) can be called in a unit test.
        public static T WithViewContext<T>(this T component) where T : ViewComponent
        {
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());

            component.ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new ViewContext { HttpContext = new DefaultHttpContext(), ViewData = viewData }
            };

            return component;
        }

        public static TModel ModelOf<TModel>(this IViewComponentResult result)
        {
            return (TModel)((ViewViewComponentResult)result).ViewData.Model;
        }
    }
}

