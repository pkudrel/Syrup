using AutoMapper;
using Syrup.Core.Local.Dto;
using Syrup.Core.Local.Models;

namespace Syrup.Core.Local.Config
{
    public class FileManagerProfile : Profile
    {
        public FileManagerProfile()
        {
            CreateMap<LocalReleaseInfo, LocalReleaseInfoDto>()
                .ForMember(dest => dest.RelaseDate, opts => opts.MapFrom(src => src.RelaseDate.ToLocalTime()));
        }

        public override string ProfileName => "FileManagerProfile";
    }
}