using RPA.MTSInspections.Models;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Mvc;

namespace RPA.MTSInspections.SL
{
    public interface IExportService
    {
        List<SelectListItem> GetMyCollection();

        string ConvertListToString(string option);

        List<ErrorLog> RunAddList(HttpPostedFileBase bulkSource);
               
    }
}