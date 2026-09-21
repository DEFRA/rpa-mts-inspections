using RPA.MTSInspections.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RPA.MTSInspections.ViewModels
{
    public class BLSSchemeView
    {
        public List<PlantOption> PlantOptions { get; set; }

        [Display(Name = "Throughput")]
        public Guid Throughput { get; set; }

        [Display(Name = "Operating Hours")]
        public Guid OperatingHours { get; set; }

        [Display(Name = "Premises Type")]
        public Guid PremisesType { get; set; }

        [Display(Name = "Type of Operation")]
        public Guid TypeOfOperation { get; set; }

        [Display(Name = "No Splitting/Relabelling (No Cutting)")]
        public SelectOption OptionANoSplit { get; set; }
        [Display(Name = "Splitting and/or Relabelling (No Cutting)")]
        public SelectOption OptionBSplit { get; set; }
        [Display(Name = "Catering")]
        public SelectOption OptionCCatering { get; set; }
        [Display(Name = "For Retail Sale")]
        public SelectOption OptionDRetail { get; set; }
        [Display(Name = "Some Cutting and Relabelling")]
        public SelectOption OptionESomeCut { get; set; }
        [Display(Name = "Wholesale")]
        public SelectOption OptionFWholesale { get; set; }      
        [Display(Name = "Cutting to Primal Stage Only")]
        public SelectOption OptionGCutting { get; set; }
        [Display(Name = "Slaughter for 3rd Party Only")]
        public SelectOption OptionHSlaughter { get; set; }

        [Display(Name = "Severity of Non-Compliance")]
        public Guid SevofNonComp { get; set; }

        [Display(Name = "Number of Inspections Resulting in Failure")]
        public Guid NumofInspFail { get; set; }

        [Display(Name = "Number of Years/Months Since Last Inspection")]
        public Guid NumYrsSinceInsp { get; set; }

        [Display(Name = "Origin of Products Handled")]
        public Guid OriginofProducts { get; set; }

        [Display(Name = "Number of Non-Compliances Encountered")]
        public Guid NumofNonComp { get; set; }

        [Display(Name = "Scheme Year")]
        public Guid SchemeYear { get; set; }

        [Display(Name = "Date Since Last Inspection")]
        public LastInspection LastInspection { get; set; }
    }

    public class SelectOption
    {
        public Option Option { get; set; }

        public bool Selected { get; set; }
    }
}