using Microsoft.AspNetCore.Mvc.Rendering;
using RPA.MTSInspections.Models;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace RPA.MTSInspections.SL
{
    public interface IExportService
    {
        List<SelectListItem> GetMyCollection();

        string ConvertListToString(string option);

        List<ErrorLog> RunAddList(IFormFile bulkSource);
               
    }
}