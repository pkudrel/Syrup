using System.Collections.Generic;
using Syrup.Common.Messaging;
using Syrup.Core.Local.Models;

namespace Syrup.Core.Local.Events
{
    public class LocalFileSystemWasSacanedEvent : IEvent
    {
        public LocalFileSystemWasSacanedEvent(List<LocalReleaseInfo> localReleaseInfos)
        {
            LocalReleaseInfos = localReleaseInfos;
        }

        public List<LocalReleaseInfo> LocalReleaseInfos { get; set; }
    }
}