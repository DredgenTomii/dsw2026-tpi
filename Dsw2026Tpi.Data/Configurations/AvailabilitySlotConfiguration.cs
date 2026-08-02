using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
    {
        builder.ToTable("AvailabilitySlots");

        builder.HasOne(s => s.Doctor)
            .WithMany()
            .HasForeignKey(s => s.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cascade: al borrar una regla (PUT que sobreescribe el mes) se borran sus slots.
        builder.HasOne(s => s.AvailabilityRule)
            .WithMany()
            .HasForeignKey(s => s.AvailabilityRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Un médico no puede tener dos slots que arranquen a la misma hora el mismo día.
        builder.HasIndex(s => new { s.DoctorId, s.Date, s.StartTime }).IsUnique();
    }
}
