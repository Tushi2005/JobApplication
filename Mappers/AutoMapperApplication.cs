using AutoMapper;
using JobApplication.DTOs.Application;
using JobApplication.Models;

namespace JobApplication.Mappers
{
    public class AutoMapperApplication: Profile
    {
        public AutoMapperApplication()
        {
            CreateMap<Application, CreateApplicationDto>();
            CreateMap<Application, ApplicationResponseDto>();
        }
    }
}
