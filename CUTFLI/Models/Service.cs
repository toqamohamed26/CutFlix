using static CUTFLI.Enums.SystemEnums;

namespace CUTFLI.Models
{
    public class Service : EntityBase
    {
        public string Name { get; set; }
        public float Price { get; set; }
        public string Description { get; set; }
        public Gender Gender { get; set; }
        public byte[] Image { get; set; }
        public List<UserService> UserServices { get; set; }
        public List<Appointment> Appointments { get; set; }
    }
}
