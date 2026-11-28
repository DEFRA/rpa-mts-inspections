using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.Services;
using RPA.MTSInspections.SL.Interfaces;

namespace RPA.MTSInspections.Pages.Admin.Bls;

[Authorize(Policy = MtsPolicies.AdminOnly)]
public class BlsCreateModel : PageModel
{
    private readonly MTSInspectionsContext db;
    private readonly ICriteriaService criteriaService;

    public BlsCreateModel(MTSInspectionsContext context, ICriteriaService criteriaService)
    {
        db = context;
        this.criteriaService = criteriaService;
    }

    [BindProperty]
    public Option Option { get; set; }

    public void OnGet()
    {
        ViewData["BlsCriteriaList"] = criteriaService.GetBlsSelectCriteria();
    }

    public IActionResult OnPost()
    {
        db.Option.Add(Option);
        db.SaveChanges();

        return RedirectToPage("/Admin/Bls/BlsOptions");
    }
}
