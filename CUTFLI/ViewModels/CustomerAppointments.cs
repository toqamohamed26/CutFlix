using CUTFLI.Enums;

namespace CUTFLI.ViewModels
{
    public class CustomerAppointments
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public SystemEnums.AppointmentStatus Status { get; set; }
        public int? UserId { get; set; }
        public Barbers User { get; set; }
        public PeopleViewModel People { get; set; }

        #region badge
        public string Color { get; set; }

        public string SetColor()
        {
            switch (this.Status)
            {
                case SystemEnums.AppointmentStatus.Checked:
                    Color = "bg-danger";
                    break;
                case SystemEnums.AppointmentStatus.Available:
                    Color = "bg-success";
                    break;
            }
            return Color;
        }
        #endregion
    }
}
