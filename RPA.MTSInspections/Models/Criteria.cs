using EF.Audit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RPA.MTSInspections.Models
{
    public class Criteria
    {
        [Key]
        public Guid CriteriaId { get; set; }
        public Guid SchemeId { get; set; }
        public string Name { get; set; }
        public int Weighting { get; set; }
        public byte Order { get; set; }
        public virtual List<Option> Option { get; set; }
        public virtual Scheme Scheme { get; set; }
    }
}