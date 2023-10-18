namespace CUTFLI.Enums
{
    public class SystemEnums
    {
        public enum Permission
        {
            Admin = 1,
            Employee = 2
        }
        
        public enum Gender
        {
            Male = 1,
            Female = 2,
        }

        public enum PaymentMethod
        {
            Cash = 1,
            CreditCard = 2,
            NetBanking = 3,
        }

        public enum AppointmentStatus
        {
            Available = 0,
            Checked = 1,
            Complete = 3,
            Canceled = 4,
            NoPresent = 5
        }
    }
}
