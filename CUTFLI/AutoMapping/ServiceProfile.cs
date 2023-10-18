using AutoMapper;
using CUTFLI.Models;
using CUTFLI.ViewModels;

namespace CUTFLI.AutoMapping
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            CreateMap<Service, ServiceViewModel>().ReverseMap();
            CreateMap<UserService, UserServiceViewModel>().ReverseMap();
        }
    }
}
