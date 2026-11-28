using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.MTSInspections.Services;
using RPA.MTSInspections.SL;

namespace RPA.MTSInspections.Pages.Export
{
    [Authorize(Policy = MtsPolicies.SupportOrAdmin)]
    public class IndexModel : PageModel
    {
        private readonly IExportService exportService;

        public IndexModel(IExportService exportService)
        {
            this.exportService = exportService;
        }

        public void OnGet(bool? upload)
        {
            ViewData["MyCustomCollection"] = exportService.GetMyCollection();

            if (upload == true)
            {
                ViewData["UploadSuccess"] = upload;
            }
        }
    }
}
