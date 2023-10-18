using System.ComponentModel.DataAnnotations;
using static CUTFLI.Enums.SystemEnums;

namespace CUTFLI.ViewModels
{
    public class ServiceViewModel
    {
        public int? Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        [Required]
        public string Name { get; set; }
        public float Price { get; set; }
        public string Description { get; set; }
        [Required]
        public Gender Gender { get; set; }
        public byte[] Image { get; set; }
    }
}
