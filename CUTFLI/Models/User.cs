using CUTFLI.Enums;
using System.ComponentModel.DataAnnotations;

namespace CUTFLI.Models
{
    public class User : EntityBase
    {
        public string FullName { get; set; }

        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*")]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public byte[] Image { get; set; }
        public SystemEnums.Permission Permission { get; set; }

        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$")]
        public string Password { get; set; }
        public List<Appointment> Appointments { get; set; }
        public List<UserService> UserServices { get; set; }
    }
}
