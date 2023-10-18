using AutoMapper;
using CUTFLI.Models;
using CUTFLI.ViewModels;

namespace CUTFLI.AutoMapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserViewModel>()
                .ForMember(x => x.Password, o => o.Ignore())
                .ReverseMap();

            CreateMap<User, Barbers>().ReverseMap();
        }
    }
}
