using EF.Audit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RPA.MTSInspections.Models
{

    public class Option
    {
        [Key]
        public Guid OptionId { get; set; }
        public Guid CriteriaId { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public bool Active { get; set; }

        public virtual Criteria Criteria { get; set; }

        public Option()
        {
            OptionId = Guid.NewGuid();
            Active = true;
        }
    }
}