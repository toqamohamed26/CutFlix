using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CUTFLI.Models;

namespace CUTFLI.Configuration
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.HasOne(x => x.User)
              .WithMany(y => y.Appointments)
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.People)
               .WithMany(y => y.Appointments)
               .HasForeignKey(x => x.VistiorId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Service)
               .WithMany(y => y.Appointments)
               .HasForeignKey(x => x.ServiceId)
               .OnDelete(DeleteBehavior.SetNull);

            builder.Property(x => x.Date).IsRequired();
            builder.Property(x => x.StartTime).IsRequired();
            builder.Property(x => x.EndTime).IsRequired();
            builder.Property(x => x.Status).IsRequired();
            builder.Property(x => x.UserId).IsRequired();
        }
    }
}
