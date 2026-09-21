using DocumentFormat.OpenXml.ExtendedProperties;
using RPA.MTSInspections.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RPA.MTSInspections.ViewModels
{
    public class SCCSchemeView
    {
        public List<PlantOption> PlantOptions { get; set; }

        [Display(Name = "Throughput")]
        [Required(ErrorMessage = "Throughput is required")]
        public Guid Throughput { get; set; }

        [Display(Name = "Operating Hours")]
        [Required(ErrorMessage = "Operating Hours is required")]
        public Guid OperatingHours { get; set; }

        [Display(Name = "Sign in Prior to Inspection")]
        [Required(ErrorMessage = "Sign in Prior to Inspection is required")]
        public Guid SignIn { get; set; }

        [Display(Name = "Trimming")]
        [Required(ErrorMessage = "Trimming is required")]
        public Guid Trimming { get; set; }

        [Display(Name = "Skin Removal")]
        [Required(ErrorMessage = "Skin removal is required")]
        public Guid SkinRemoval { get; set; }

        [Display(Name = "Carcass Dressing")]
        [Required(ErrorMessage = "Carcass Dressing is required")]
        public Guid CarcassDressing { get; set; }

        [Display(Name = "Head Removal")]
        [Required(ErrorMessage = "Head removal is required")]
        public Guid HeadRemoval { get; set; }

        [Display(Name = "Feet removal")]
        [Required(ErrorMessage = "Feet removal is required")]
        public Guid FeetRemoval { get; set; }

        [Display(Name = "Scheme Year")]
        public Guid SchemeYear { get; set; }

        [Display(Name = "Date Since Last Inspection")]
        public LastInspection LastInspection { get; set; }

        [Display(Name = "Failed / Unsatisfactory")]
        [Required(ErrorMessage = "Failed / unsatisfactory is required")]
        public Guid FailedUnsatisfactory { get; set; }

        [Display(Name = "How many fails / unsatisfactory in previous 12 Months")]
        [Required(ErrorMessage = "How many fails / unsatisfactory in previous 12 Months is required")]
        public Guid FailedUnsatisfactoryInLast12Months { get; set; }

        [Display(Name = "How many near misses in previous 12 Months")]
        [Required(ErrorMessage = "How many fails / unsatisfactory in previous 12 Months is required")]
        public Guid NearMissesInLast12Months { get; set; }

        [Display(Name = "Number of Enforcement Notices Issued in Last 12 Months")]
        [Required(ErrorMessage = "Number of Enforcement Notices Issued in Last 12 Months is required")]
        public Guid EnforcementNoticesIssuedLast12Months { get; set; }

        [Display(Name = "Number of Penalty Notices Issued in Last 12 Months")]
        [Required(ErrorMessage = "Number of Penalty Notices Issued in Last 12 Months is required")]
        public Guid NumberPenaltyNoticesIssuedInLast12Months { get; set; }
    }
}