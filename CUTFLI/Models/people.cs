using System.ComponentModel.DataAnnotations;

namespace CUTFLI.Models
{
    public class People : EntityBase
    {
        public string FullName { get; set; }

        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*")]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Note { get; set; }
        public List<Appointment> Appointments { get; set; }
    }
}
