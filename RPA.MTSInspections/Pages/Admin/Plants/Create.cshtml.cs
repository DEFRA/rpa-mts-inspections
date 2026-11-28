using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.Services;

namespace RPA.MTSInspections.Pages.Admin.Plants
{
    [Authorize(Policy = MtsPolicies.SupportOrAdmin)]
    public class CreateModel : PageModel
    {
        private readonly MTSInspectionsContext db;

        public CreateModel(MTSInspectionsContext db)
        {
            this.db = db;
        }

        [BindProperty]
        public Plant Plant { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            db.Plant.Add(Plant);
            db.SaveChanges();

            return RedirectToPage("/Admin/Plants/Index");
        }
    }
}
