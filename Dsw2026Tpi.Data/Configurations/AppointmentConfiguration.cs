using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Dsw2026Tpi.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.Property(a => a.Reason)
            .IsRequired()
            .HasMaxLength(300);

        var statusConverter = new ValueConverter<AppointmentStatus, string>(
            status => status == AppointmentStatus.Booked ? "BOOKED"
                    : status == AppointmentStatus.Cancelled ? "CANCELLED"
                    : status == AppointmentStatus.Attended ? "ATTENDED"
                    : "NO_SHOW",
            texto => texto == "BOOKED" ? AppointmentStatus.Booked
                   : texto == "CANCELLED" ? AppointmentStatus.Cancelled
                   : texto == "ATTENDED" ? AppointmentStatus.Attended
                   : AppointmentStatus.NoShow);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(statusConverter);

        builder.HasOne(a => a.AvailabilitySlot)
            .WithMany()
            .HasForeignKey(a => a.AvailabilitySlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.AvailabilitySlotId)
            .IsUnique()
            .HasFilter("[Status] = 'BOOKED'");
    }
}