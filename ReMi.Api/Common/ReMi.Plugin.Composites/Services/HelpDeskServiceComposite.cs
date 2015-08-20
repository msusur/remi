using ReMi.Contracts.Plugins.Data;
using ReMi.Contracts.Plugins.Data.HelpDesk;
using ReMi.Contracts.Plugins.Services.HelpDesk;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;

namespace ReMi.Plugin.Composites.Services
{
    public class HelpDeskServiceComposite : BaseComposit<IHelpDeskService>, IHelpDeskService
    {
        public HelpDeskTicket CreateTicket(HelpDeskTicket ticket, IEnumerable<Guid> packageIds)
        {
            var enumerable = packageIds as Guid[] ?? packageIds.ToArray();
            return GetPackageServicesWithConfiguration(enumerable, PluginType.HelpDesk)
                .Select(x => x.Service.CreateTicket(ticket, enumerable))
                .FirstOrDefault();
        }

        public HelpDeskTicket UpdateTicket(HelpDeskTicket ticket, IEnumerable<Guid> packageIds)
        {
            var enumerable = packageIds as Guid[] ?? packageIds.ToArray();
            return GetPackageServicesWithConfiguration(enumerable, PluginType.HelpDesk)
                .Select(x => x.Service.UpdateTicket(ticket, enumerable))
                .FirstOrDefault();
        }

        public void DeleteTicket(string ticketRef, IEnumerable<Guid> packageIds)
        {
            var enumerable = packageIds as Guid[] ?? packageIds.ToArray();
            GetPackageServicesWithConfiguration(enumerable, PluginType.HelpDesk)
                .Each(x => x.Service.DeleteTicket(ticketRef, enumerable));
        }
    }
}
