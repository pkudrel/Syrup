using MediatR;
using Syrup.Core.Local.Dto;

namespace Syrup.Core.Local.Commands
{
    public class MakeActiveSelectedReleaseCommand :  INotification
    {
        public LocalReleaseInfoDto LocalReleaseInfoDto { get; set; }
      

        public MakeActiveSelectedReleaseCommand(LocalReleaseInfoDto localReleaseInfoDto)
        {
            LocalReleaseInfoDto = localReleaseInfoDto;
           
        }
    }
}