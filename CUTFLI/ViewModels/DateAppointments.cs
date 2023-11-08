namespace CUTFLI.ViewModels
{
    public class DateAppointments
    {
        public DateTime StartDate { get; set; }
        public string DayName { get; set; }
        public string DayNumber { get; set; }
        public List<StartTimeViewModel> Appointments { get; set; }

    }
}
