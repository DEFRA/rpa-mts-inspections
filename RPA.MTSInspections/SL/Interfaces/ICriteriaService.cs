using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using RPA.MTSInspections.Models;
using X.PagedList;
using Microsoft.AspNetCore.Mvc;

namespace RPA.MTSInspections.SL.Interfaces
{
    public interface ICriteriaService
    {
        List<SelectListItem> GetBlsSelectCriteria();

        List<Criteria> GetBlsCriteria();

        List<SelectListItem> GetBccSelectCriteria();

        List<Criteria> GetBccCriteria();

        List<SelectListItem> GetPcgSelectCriteria();

        List<Criteria> GetPcgCriteria();

        List<Criteria> GetSccCriteria();

        List<SelectListItem> GetSccSelectCriteria();
    }
}