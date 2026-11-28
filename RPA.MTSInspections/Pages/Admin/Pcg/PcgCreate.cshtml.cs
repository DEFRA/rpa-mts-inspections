using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.Services;
using RPA.MTSInspections.SL.Interfaces;

namespace RPA.MTSInspections.Pages.Admin.Pcg;

[Authorize(Policy = MtsPolicies.AdminOnly)]
public class PcgCreateModel : PageModel
{
    private readonly MTSInspectionsContext db;
    private readonly ICriteriaService criteriaService;

    public PcgCreateModel(MTSInspectionsContext context, ICriteriaService criteriaService)
    {
        db = context;
        this.criteriaService = criteriaService;
    }

    [BindProperty]
    public Option Option { get; set; }

    public void OnGet()
    {
        ViewData["PcgCriteriaList"] = criteriaService.GetPcgSelectCriteria();
    }

    public IActionResult OnPost()
    {
        db.Option.Add(Option);
        db.SaveChanges();

        return RedirectToPage("/Admin/Pcg/PcgOptions");
    }
}
