using EF.Audit;
using RPA.MTSInspections.ViewModels;
using System;

namespace RPA.MTSInspections.SL
{
    public interface IAuditService
    {
        void Log(Guid plantId, string user, string action, string propertyName, string value, string newValue = null);
    }
}