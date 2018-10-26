using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Optional;
using Syrup.Core.Local.Exceptions;
using Syrup.Core.Local.Models;
using Syrup.Core.Server.Models;

namespace Syrup.Core.Local.Services
{
    public interface ILocalReleaseService
    {
        void MakeUpdateFromServer(List<ReleaseInfo> relaseInfos);
        void MakeUpdateFromLocal(List<LocalReleaseInfo> relaseInfos);
        List<LocalReleaseInfo> GetLocalReleaseList();
        Option<LocalReleaseInfo> TryGetReleaseByName(string name);
        void MakeActive(LocalReleaseInfo release);
      
    }

    public class LocalReleaseService : ILocalReleaseService
    {
        private readonly ConcurrentDictionary<string, LocalReleaseInfo> _releaseInfoLocals =
            new ConcurrentDictionary<string, LocalReleaseInfo>(StringComparer.OrdinalIgnoreCase);

       

        public void MakeUpdateFromServer(List<ReleaseInfo> relaseInfos)
        {
            _releaseInfoLocals.ForEach(x => x.Value.IsOnServer = false);
            foreach (var releaseInfo in relaseInfos)
            {
                LocalReleaseInfo ri;
                if (_releaseInfoLocals.TryGetValue(releaseInfo.Name, out ri))
                {
                    ri.IsOnServer = true;
                    ri.App = releaseInfo.App;
                    ri.File = releaseInfo.File;
                    ri.Channel = releaseInfo.Channel;
                    ri.FileUrl = releaseInfo.FileUrl;
                    ri.RelaseDate = releaseInfo.RelaseDate;
                    ri.SemVer = releaseInfo.SemVer;
                    ri.Sha = releaseInfo.Sha;
                }
                else
                {
                    var rl = new LocalReleaseInfo
                    {
                        App = releaseInfo.App,
                        Name = releaseInfo.Name,
                        Channel = releaseInfo.Channel,
                        File = releaseInfo.File,
                        FileUrl = releaseInfo.FileUrl,
                        SemVer = releaseInfo.SemVer,
                        Sha = releaseInfo.Sha,
                        RelaseDate = releaseInfo.RelaseDate,
                        IsOnServer = true
                    };
                    if (!_releaseInfoLocals.TryAdd(releaseInfo.Name, rl))
                    {
                        throw new CannotAddToLocalRelaseInfoServiceException();
                    }
                }
            }
        }

        public List<LocalReleaseInfo> GetLocalReleaseList()
        {
            return _releaseInfoLocals.Select(x => x.Value).OrderByDescending(x => x.RelaseDate).ToList();
        }

        public Option<LocalReleaseInfo> TryGetReleaseByName(string name)
        {
            LocalReleaseInfo releaseInfo;
            return _releaseInfoLocals.TryGetValue(name, out releaseInfo)
                ? Option.Some(releaseInfo)
                : Option.None<LocalReleaseInfo>();
        }

        public void MakeActive(LocalReleaseInfo release)
        {
            _releaseInfoLocals.ForEach(x=>x.Value.IsActive = false);
            var r = _releaseInfoLocals.FirstOrDefault(x => x.Key == release.Name).Value;
            if (r!=null)
            {
                r.IsActive = true;
            }
        }

        public void DeleteOldLocalReleases(List<LocalReleaseInfo> notificationLocalReleaseInfos)
        {
            throw new NotImplementedException();
        }

        public void MakeUpdateFromLocal(List<LocalReleaseInfo> relaseInfos)
        {
            foreach (var releaseInfo in relaseInfos)
            {
                LocalReleaseInfo ri;
                if (_releaseInfoLocals.TryGetValue(releaseInfo.Name, out ri))
                {
                    ri.App = releaseInfo.App;
                    ri.IsOnServer = true;
                    ri.File = releaseInfo.File;
                    ri.Channel = releaseInfo.Channel;
                    ri.FileUrl = releaseInfo.FileUrl;
                    ri.RelaseDate = releaseInfo.RelaseDate;
                    ri.SemVer = releaseInfo.SemVer;
                    ri.Sha = releaseInfo.Sha;
                    ri.IsLocalNuget = releaseInfo.IsLocalNuget;
                    ri.IsExtracted = releaseInfo.IsExtracted;
                    ri.IsActive = releaseInfo.IsActive;
                }
                else
                {
                    var rl = new LocalReleaseInfo
                    {
                        App =  releaseInfo.App,
                        Name = releaseInfo.Name,
                        File = releaseInfo.File,
                        SemVer = releaseInfo.SemVer,
                        Sha = releaseInfo.Sha,
                        RelaseDate = releaseInfo.RelaseDate,
                        IsLocalNuget = releaseInfo.IsLocalNuget,
                        IsExtracted = releaseInfo.IsExtracted,
                        IsActive = releaseInfo.IsActive
                    };
                    if (!_releaseInfoLocals.TryAdd(releaseInfo.Name, rl))
                    {
                        throw new CannotAddToLocalRelaseInfoServiceException();
                    }
                }
            }
        }
    }
}