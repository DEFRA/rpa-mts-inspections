using EF.Audit;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Helpers;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.MTSInspections.SL
{
    public class AuditService : IAuditService
    {
        MTSInspectionsContext db;

        public AuditService(MTSInspectionsContext context)
        {
            db = context;
        }


        public void Log(Guid plantId, string user, string action, string propertyName, string value, string newValue = null)
        {
            Audit audit = new Audit(plantId, user, action, propertyName, value, newValue);
            db.Audit.Add(audit);
            
        }

    }
}