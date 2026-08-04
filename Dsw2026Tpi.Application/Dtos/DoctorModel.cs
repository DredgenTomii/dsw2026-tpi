namespace Dsw2026Tpi.Application.Dtos;

// Sin cambios respecto de lo que ya estaba en el repo (Request/Response ya
// tenían justo los campos que hacían falta para POST/PUT). Se deja completo
// para que quede claro que no hay que tocar nada acá.
public record DoctorModel
{
    public record Request(string Name, string LicenseNumber, Guid SpecialityId);
    public record Response(Guid Id, string Name, string LicenseNumber, SpecialityDto? Speciality);
    public record SpecialityDto(Guid? SpecialityId, string? Name);
}
