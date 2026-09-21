using System;
using System.Collections.Generic;
using RPA.MTSInspections.Models;
using PagedList;
using System.Web.Mvc;

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