namespace CUTFLI.ViewModels
{
    public class ServiceWithAppointment
    {
        public int? ServiceId { get; set; }
        public List<CustomerAppointments> CustomerAppointments { get; set; }
    }
}
