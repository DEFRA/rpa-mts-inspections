using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.Services;

namespace RPA.MTSInspections.Pages.Admin
{
    [Authorize(Policy = MtsPolicies.AdminOnly)]
    public class EditWeightingScoreModel : PageModel
    {
        private readonly MTSInspectionsContext db;

        public EditWeightingScoreModel(MTSInspectionsContext db)
        {
            this.db = db;
        }

        [BindProperty]
        public Criteria Criteria { get; set; }

        public IActionResult OnGet(Guid criteriaId)
        {
            Criteria = db.Criteria.AsNoTracking().Include(x => x.Scheme).FirstOrDefault(x => x.CriteriaId == criteriaId);

            if (Criteria == null)
            {
                return NotFound();
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            Criteria criteriaChange = db.Criteria.FirstOrDefault(x => x.CriteriaId == Criteria.CriteriaId);

            if (criteriaChange == null)
            {
                return NotFound();
            }

            criteriaChange.Weighting = Criteria.Weighting;

            db.SetModified(criteriaChange);
            db.SaveChanges();

            string scheme = db.Scheme.Where(x => x.SchemeId == criteriaChange.SchemeId).Select(x => x.Name).FirstOrDefault();

            return scheme switch
            {
                "BLS" => RedirectToPage("/Admin/Bls/BlsOptions"),
                "BCC" => RedirectToPage("/Admin/Bcc/BccOptions"),
                "SCC" => RedirectToPage("/Admin/Scc/SccOptions"),
                _ => RedirectToPage("/Admin/Pcg/PcgOptions")
            };
        }
    }
}
