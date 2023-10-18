using AutoMapper;
using CUTFLI.Models;
using CUTFLI.ViewModels;

namespace CUTFLI.AutoMapping
{
    public class ContactUsProfile : Profile
    {
        public ContactUsProfile()
        {
            CreateMap<ContactUsViewModel, ContactUs>().ReverseMap();
        }
    }
}
