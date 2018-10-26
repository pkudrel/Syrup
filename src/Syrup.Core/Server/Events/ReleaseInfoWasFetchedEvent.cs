using System.Collections.Generic;
using Syrup.Common.Messaging;
using Syrup.Core.Server.Models;

namespace Syrup.Core.Server.Events
{
    public class ReleaseInfoWasFetchedEvent : IEvent
    {
        public ReleaseInfoWasFetchedEvent(List<ReleaseInfo> relaseInfos)
        {
            RelaseInfos = relaseInfos;
        }

        public List<ReleaseInfo> RelaseInfos { get; }
    }
}