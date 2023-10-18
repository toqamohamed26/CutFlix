using CUTFLI.Configuration;
using CUTFLI.Enums;
using Microsoft.EntityFrameworkCore;

namespace CUTFLI.Models
{
    public class CUTFLIDbContext : DbContext
    {
        public CUTFLIDbContext(DbContextOptions<CUTFLIDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<People> Peoples { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<ContactUs> ContactUs { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<UserService> UserServices { get; set; }
        public DbSet<Video> Videos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppointmentConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PeopleConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContactUsConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserServiceConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(VideoConfiguration).Assembly);

            modelBuilder.Entity<User>(o =>
            {
                o.HasData(new User()
                {
                    Id = 1,
                    FullName = "Cutflix Admin",
                    Email = "cutflixshop@gmail.com",
                    Password = "AQAAAAEAACcQAAAAEL4OkpScomcQ8y0KPpodA16wC7jGznr8AxV1UolMwkFvTbbQO6tX7WVQ5lKD48YFgg==",//P@ssw0rd!@#
                    Permission = SystemEnums.Permission.Admin,
                    Address = "US",
                    PhoneNumber = "14145200699",
                    CreatedDate = DateTime.Now,
                    Image = null
                });
            });
        }
    }
}
