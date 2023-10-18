namespace CUTFLI.ViewModels
{
    public class UserServiceViewModel
    {
        public int UserId { get; set; }
        public int ServiceId { get; set; }
        public UserViewModel User { get; set; }
        public ServiceViewModel Service { get; set; }

    }
}
