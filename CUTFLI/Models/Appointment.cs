using CUTFLI.Enums;

namespace CUTFLI.Models
{
    public class Appointment : EntityBase
    {
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public SystemEnums.AppointmentStatus Status { get; set; }
        public int? VistiorId { get; set; }
        public int? UserId { get; set; }
        public float? Price { get; set; }
        public SystemEnums.PaymentMethod? PaymentMethod { get; set; }
        public virtual People People { get; set; }
        public virtual User User { get; set; }
        public int? ServiceId { get; set; }
        public virtual Service Service { get; set; }
    }
}
