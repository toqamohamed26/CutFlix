using System.ComponentModel.DataAnnotations;

namespace CUTFLI.ViewModels
{
    public class PasswordModel
    {
        public int Id { get; set; }
        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$")]
        [MaxLength(25)]
        public string NewPassword { get; set; }
        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$")]
        [MaxLength(25)]
        public string ConfirmedPassword { get; set; }

    }
}
