using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.Services;
using RPA.MTSInspections.SL.Interfaces;

namespace RPA.MTSInspections.Pages.Admin.Bls;

[Authorize(Policy = MtsPolicies.AdminOnly)]
public class BlsOptionsModel : PageModel
{
    private readonly ICriteriaService criteriaService;

    public BlsOptionsModel(ICriteriaService criteriaService)
    {
        this.criteriaService = criteriaService;
    }

    public List<Criteria> CriteriaList { get; private set; }

    public void OnGet()
    {
        CriteriaList = criteriaService.GetBlsCriteria();
    }
}
