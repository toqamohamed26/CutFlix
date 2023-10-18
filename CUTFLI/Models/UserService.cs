namespace CUTFLI.Models
{
    public class UserService : EntityBase
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int ServiceId { get; set; }
        public virtual Service Service { get; set; }

    }
}
