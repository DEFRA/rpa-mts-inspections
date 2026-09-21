using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RPA.MTSInspections.Models
{
    public class Audit
    {
        public int Id { get; set; }


        public Guid PlantId { get; set; }
        [Display(Name = "Date Changed")]

        public DateTime Date { get; set; }
        [Display(Name = "User")]
        public string User { get; set; }
        [Display(Name = "Area Changed")]
        public string Action { get; set; }
        [Display(Name = "Field Altered")]
        public string PropertyName { get; set; }
        [Display(Name = "Old Value")]
        public string Value { get; set; }
        [Display(Name = "New Value")]
        public string NewValue { get; set; }

        public Audit() { }

        public Audit(Guid plantId, string user, string action, string propertyName, string value, string newValue = null)
        {
            PlantId = plantId;
            Date = DateTime.UtcNow;
            User = user;
            Action = action;
            PropertyName = propertyName;
            Value = value;
            NewValue = newValue;
        }

    }
}