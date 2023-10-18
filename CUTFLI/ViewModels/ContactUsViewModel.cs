using System.ComponentModel.DataAnnotations;

namespace CUTFLI.ViewModels
{
    public class ContactUsViewModel
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
        [Required]
        public string Message { get; set; }
    }
}
