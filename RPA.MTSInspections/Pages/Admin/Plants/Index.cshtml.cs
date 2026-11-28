using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.Services;
using RPA.MTSInspections.SL;
using X.PagedList;

namespace RPA.MTSInspections.Pages.Admin.Plants
{
    [Authorize(Policy = MtsPolicies.SupportOrAdmin)]
    public class IndexModel : PageModel
    {
        private readonly IPlantService plantService;

        public IndexModel(IPlantService plantService)
        {
            this.plantService = plantService;
        }

        public IPagedList<Plant> Plants { get; private set; }

        public void OnGet(string searchstring = null, int page = 1, int pageSize = 15)
        {
            try
            {
                Plants = plantService.GetPlant(searchstring, page, pageSize, false);
                ViewData["searchData"] = searchstring;
            }
            catch (Exception ex)
            {
                throw new Exception("PL01: Unable to return Plants", ex);
            }
        }
    }
}
