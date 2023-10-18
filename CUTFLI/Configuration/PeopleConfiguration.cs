using CUTFLI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CUTFLI.Configuration
{
    public class PeopleConfiguration : IEntityTypeConfiguration<People>
    {
        public void Configure(EntityTypeBuilder<People> builder)
        {
            builder.Property(x => x.FullName).IsRequired();
            builder.Property(x => x.PhoneNumber).IsRequired();
        }
    }
}
