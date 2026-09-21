using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RPA.MTSInspections.Models
{
    public class ErrorLog
    {
        [Display(Name = "Licence Number")]
        public string LicenceNo { get; set;}
        public string Scheme { get; set; }

        [Display(Name = "Error Description")]
        public string Error { get; set; }
    }
}