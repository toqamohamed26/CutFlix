using CUTFLI.Enums;
using System.ComponentModel.DataAnnotations;

namespace CUTFLI.ViewModels
{
    public class UserViewModel
    {
        public int? Id { get; set; }
        public DateTime? CreatedDate { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Email invalid")]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public byte[] Image { get; set; }
        public SystemEnums.Permission Permission { get; set; }

        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$")]
        [MaxLength(25)]
        public string Password { get; set; }
        public List<int> Services { get; set; }    
        public List<UserServiceViewModel> UserServices { get; set; }
    }
}
