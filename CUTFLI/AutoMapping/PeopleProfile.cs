using AutoMapper;
using CUTFLI.Models;
using CUTFLI.ViewModels;

namespace CUTFLI.AutoMapping
{
    public class PeopleProfile : Profile
    {
        public PeopleProfile()
        {
            CreateMap<People, PeopleViewModel>().ReverseMap();
        }
    }
}
