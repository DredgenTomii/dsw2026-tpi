using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");

        builder.HasIndex(d => d.LicenseNumber).IsUnique();

        builder.Property(d => d.Deleted)
            .HasDefaultValue(false);

        builder.HasQueryFilter(d => !d.Deleted);
    }
}
