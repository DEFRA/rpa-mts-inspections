using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.Services;
using RPA.MTSInspections.SL.Interfaces;

namespace RPA.MTSInspections.Pages.Admin.Scc;

[Authorize(Policy = MtsPolicies.AdminOnly)]
public class SccCreateModel : PageModel
{
    private readonly MTSInspectionsContext db;
    private readonly ICriteriaService criteriaService;

    public SccCreateModel(MTSInspectionsContext context, ICriteriaService criteriaService)
    {
        db = context;
        this.criteriaService = criteriaService;
    }

    [BindProperty]
    public Option Option { get; set; }

    public void OnGet()
    {
        ViewData["SccCriteriaList"] = criteriaService.GetSccSelectCriteria();
    }

    public IActionResult OnPost()
    {
        db.Option.Add(Option);
        db.SaveChanges();

        return RedirectToPage("/Admin/Scc/SccOptions");
    }
}
