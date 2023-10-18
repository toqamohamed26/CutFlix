using System.ComponentModel.DataAnnotations.Schema;

namespace CUTFLI.Models
{
    public class EntityBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? LastModifiedDate { get; set; }  
        public int? LastModifiedBy { get; set; }  
    }
}
