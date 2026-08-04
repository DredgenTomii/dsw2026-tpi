namespace Dsw2026Tpi.Application.Dtos;

public class AppointmentModel
{
    public record PatientRequest(long Dni);

    public record Request(
        Guid DoctorId,
        Guid AvailabilitySlotId,
        PatientRequest Patient,
        string Reason);

    public record SpecialityResponse(Guid SpecialtyId, string Name);

    public record DoctorResponse(Guid DoctorId, string Name, SpecialityResponse Specialty);

    public record PatientResponse(long Dni, string FullName);

    public record Response(
        Guid AppointmentsId,
        string AppointmentsStatus,
        PatientResponse Patient,
        DoctorResponse Doctor);
}