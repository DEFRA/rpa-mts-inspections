using RPA.MTSInspections.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RPA.MTSInspections.ViewModels
{
    public class BCCSchemeView
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

        [Display(Name = "Record Keeping")]
        [Required(ErrorMessage = "Record Keeping is required")]
        public Guid RecordKeeping { get; set; }

        [Display(Name = "Clearing of Lines")]
        [Required(ErrorMessage = "Title is required")]
        public Guid LineClearing { get; set; }

        [Display(Name = "Carcass Hanging Method")]
        [Required(ErrorMessage = "Carcass Hanging Method is required")]
        public Guid CarcassHang { get; set; }

        [Display(Name = "Trimming")]
        [Required(ErrorMessage = "Trimming is required")]
        public Guid Trimming { get; set; }

        [Display(Name = "Hide Puller")]
        [Required(ErrorMessage = "Hide Puller is required")]
        public Guid HidePuller { get; set; }

        [Display(Name = "Carcass Dressing")]
        [Required(ErrorMessage = "Carcass Dressing is required")]
        public Guid CarcassDress { get; set; }

        [Display(Name = "Number of Weeks Since Last Inspection")]
        public Guid NumofWeeks { get; set; }

        [Display(Name = "Failed/Unsatisfactory on Last Visit")]
        public Guid FailedVisit { get; set; }

        [Display(Name = "How Many Fails/Unsatisfactory in Previous 3 Years")]
        public Guid FailedLastThree { get; set; }

        [Display(Name = "How Many Near Misses in Previous 3 Years")]
        public Guid MissLastThree { get; set; }

        [Display(Name = "Scheme Year")]
        public Guid SchemeYear { get; set; }

        [Display(Name = "Date Since Last Inspection")]
        public LastInspection LastInspection { get; set; }
    }
}