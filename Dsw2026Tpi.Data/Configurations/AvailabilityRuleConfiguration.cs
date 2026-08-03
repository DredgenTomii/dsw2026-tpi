using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class AvailabilityRuleConfiguration : IEntityTypeConfiguration<AvailabilityRule>
{
    public void Configure(EntityTypeBuilder<AvailabilityRule> builder)
    {
        builder.ToTable("AvailabilityRules");

        builder.Property(r => r.DayOfWeek)
            .HasConversion<int>();

        builder.HasOne(r => r.Doctor)
            .WithMany()
            .HasForeignKey(r => r.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ayuda a la query de "ya existe configuración para este médico/mes"
        builder.HasIndex(r => new { r.DoctorId, r.Year, r.Month });
    }
}
