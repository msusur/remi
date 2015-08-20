using System;
using System.Collections.Generic;
using ReMi.Contracts.Plugins.Data.ReleaseContent;

namespace ReMi.Contracts.Plugins.Services.ReleaseContent
{
    public interface IReleaseContent : IPluginService
    {
        IEnumerable<ReleaseContentTicket> GetTickets(IEnumerable<Guid> packageIds);
        IEnumerable<ReleaseContentTicket> GetDefectTickets(IEnumerable<Guid> packageIds);

        void UpdateTicket(IEnumerable<ReleaseContentTicket> tickets, Guid packageId);

    }
}
