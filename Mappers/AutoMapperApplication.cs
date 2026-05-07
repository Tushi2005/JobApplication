using AutoMapper;
using JobApplication.DTOs.Application;
using JobApplication.Models;

namespace JobApplication.Mappers
{
    public class AutoMapperApplication: Profile
    {
        public AutoMapperApplication()
        {
            CreateMap<CreateApplicationDto, Application >();
            CreateMap<Application, ApplicationResponseDto>();
            CreateMap<Application, UpdateApplicationDto>().ReverseMap();
        }
    }
}
