using System.ComponentModel.DataAnnotations;

namespace CUTFLI.ViewModels
{
    public class PeopleViewModel
    {
        public int? Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*")]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public int AppointmentId { get; set; }
    }
}
