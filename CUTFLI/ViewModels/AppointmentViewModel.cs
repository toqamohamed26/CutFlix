using CUTFLI.Enums;
using CUTFLI.Models;
using System.ComponentModel;

namespace CUTFLI.ViewModels
{
    public class AppointmentViewModel
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public SystemEnums.AppointmentStatus Status { get; set; }
        public int? VistiorId { get; set; }
        public int? ServiceId { get; set; }
        public int UserId { get; set; }
        public float? Price { get; set; }
        public SystemEnums.PaymentMethod? PaymentMethod { get; set; }
        public PeopleViewModel People { get; set; }
        public UserViewModel User { get; set; }
        public ServiceViewModel Service { get; set; }
        #region badge
        public string Color { get; set; }

        public string SetColor()
        {
            switch (this.Status)
            {
                case SystemEnums.AppointmentStatus.Checked:
                    Color = "bg-primary";
                    break;
                case SystemEnums.AppointmentStatus.Available:
                    Color = "bg-success";
                    break;
                case SystemEnums.AppointmentStatus.Complete:
                    Color = "bg-info";
                    break;
                case SystemEnums.AppointmentStatus.Canceled:
                    Color = "bg-danger";
                    break;
                case SystemEnums.AppointmentStatus.NoPresent:
                    Color = "bg-secondary";
                    break;
            }
            return Color;
        }
        #endregion
    }
}
