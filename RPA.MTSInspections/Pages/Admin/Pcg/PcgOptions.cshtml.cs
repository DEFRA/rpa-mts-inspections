using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.Services;
using RPA.MTSInspections.SL.Interfaces;

namespace RPA.MTSInspections.Pages.Admin.Pcg;

[Authorize(Policy = MtsPolicies.AdminOnly)]
public class PcgOptionsModel : PageModel
{
    private readonly ICriteriaService criteriaService;

    public PcgOptionsModel(ICriteriaService criteriaService)
    {
        this.criteriaService = criteriaService;
    }

    public List<Criteria> CriteriaList { get; private set; }

    public void OnGet()
    {
        CriteriaList = criteriaService.GetPcgCriteria();
    }
}
