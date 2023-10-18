using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CUTFLI.Models;

namespace CUTFLI.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(x => x.FullName).IsRequired().HasMaxLength(60);
            builder.Property(x => x.Address).IsRequired().HasMaxLength(60);
            builder.Property(x => x.Email).IsRequired().HasMaxLength(40);
            builder.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(15);
            builder.Property(x => x.Password).IsRequired().HasMaxLength(350);
            builder.Property(x => x.Permission).IsRequired();
        }
    }
}
