using AutoMapper;
using CUTFLI.Models;
using CUTFLI.ViewModels;

namespace CUTFLI.AutoMapping
{
    public class AppointmentProfile : Profile
    {
        public AppointmentProfile()
        {
            CreateMap<Appointment, AppointmentViewModel>().ReverseMap();
            CreateMap<Appointment, CustomerAppointments>().ReverseMap();
        }
    }
}
