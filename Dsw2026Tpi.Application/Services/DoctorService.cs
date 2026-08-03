using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IPersistence _persistence;

    public DoctorService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<DoctorModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
        var doctors = await _persistence.Paginate<Doctor, string>(pageSize, pageIndex, d => string.IsNullOrWhiteSpace(name) ||
                                                   d.Name.Contains(name), x => x.Name, nameof(Doctor.Speciality));

        return doctors.Map(ToResponse);
    }

    public async Task<DoctorModel.Response> Add(DoctorModel.Request request)
    {
        ValidateRequest(request);

        var speciality = await _persistence.GetById<Speciality>(request.SpecialityId)
            ?? throw new EntityNotFoundException(nameof(Speciality));

        await EnsureLicenseNumberIsFree(request.LicenseNumber);

        var doctor = new Doctor(request.Name, request.LicenseNumber, speciality);
        await _persistence.Add(doctor);

        return ToResponse(doctor);
    }

    public async Task<DoctorModel.Response> Update(Guid id, DoctorModel.Request request)
    {
        ValidateRequest(request);

        var doctor = await _persistence.GetById<Doctor>(id, nameof(Doctor.Speciality))
            ?? throw new EntityNotFoundException(nameof(Doctor));

        await EnsureLicenseNumberIsFree(request.LicenseNumber, excludingDoctorId: id);

        var speciality = await _persistence.GetById<Speciality>(request.SpecialityId)
            ?? throw new EntityNotFoundException(nameof(Speciality));

        // Se muta la MISMA instancia trackeada por EF (Name/LicenseNumber ya no son
        // "init"), en vez de crear un Doctor nuevo, para evitar el error de EF
        // "another instance with the same key value is already being tracked".
        doctor.Update(request.Name, request.LicenseNumber, speciality);
        await _persistence.Update(doctor);

        return ToResponse(doctor);
    }

    public async Task Delete(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(id)
            ?? throw new EntityNotFoundException(nameof(Doctor));

        // Soft delete: la entidad Doctor ya traía IsActive/Deactivate() pensado para esto.
        doctor.Deactivate();
        await _persistence.Update(doctor);
    }

    private async Task EnsureLicenseNumberIsFree(string licenseNumber, Guid? excludingDoctorId = null)
    {
        var existing = await _persistence.First<Doctor>(d =>
            d.LicenseNumber == licenseNumber && (excludingDoctorId == null || d.Id != excludingDoctorId));

        if (existing is not null)
            throw new ConflictException("DOCTOR_LICENSE_CONFLICT", "Ya existe un médico con esa matrícula");
    }

    private static void ValidateRequest(DoctorModel.Request request)
    {
        var errors = new List<(string, string)>();

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length is < 3 or > 100)
            errors.Add((nameof(request.Name), "El nombre debe tener entre 3 y 100 caracteres"));

        if (string.IsNullOrWhiteSpace(request.LicenseNumber) || request.LicenseNumber.Length is < 3 or > 20)
            errors.Add((nameof(request.LicenseNumber), "La matrícula debe tener entre 3 y 20 caracteres"));

        if (request.SpecialityId == Guid.Empty)
            errors.Add((nameof(request.SpecialityId), "Debe indicar una especialidad válida"));

        if (errors.Count > 0)
            throw new ValidationException().WithDetail(errors);
    }

    private static DoctorModel.Response ToResponse(Doctor doctor) =>
        new(doctor.Id, doctor.Name, doctor.LicenseNumber,
            new DoctorModel.SpecialityDto(doctor.Speciality?.Id, doctor.Speciality?.Name));
}
