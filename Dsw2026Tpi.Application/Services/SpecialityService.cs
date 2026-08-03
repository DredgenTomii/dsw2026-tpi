using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class SpecialityService : ISpecialityService
{
    private readonly IPersistence _persistence;

    public SpecialityService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<SpecialityModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
        ValidateNameFilter(name);

        var specialities = await _persistence.Paginate<Speciality, string>(pageSize, pageIndex,
            s => string.IsNullOrWhiteSpace(name) || s.Name.Contains(name), s => s.Name);

        return specialities.Map(ToResponse);
    }

    public async Task<SpecialityModel.Response> Add(SpecialityModel.Request request)
    {
        Validate(request);

        var speciality = new Speciality(request.Name, request.Description);
        await _persistence.Add(speciality);

        return ToResponse(speciality);
    }

    public async Task<SpecialityModel.Response> Update(Guid id, SpecialityModel.Request request)
    {
        Validate(request);

        var speciality = await _persistence.GetById<Speciality>(id)
            ?? throw new EntityNotFoundException(nameof(Speciality));

        speciality.Update(request.Name, request.Description);
        await _persistence.Update(speciality);

        return ToResponse(speciality);
    }

    public async Task Delete(Guid id)
    {
        var speciality = await _persistence.GetById<Speciality>(id)
            ?? throw new EntityNotFoundException(nameof(Speciality));

        speciality.Delete();
        await _persistence.Update(speciality);
    }

    private static void Validate(SpecialityModel.Request request)
    {
        var errors = new List<(string Field, string Issue)>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add((nameof(request.Name), "El nombre es obligatorio"));
        else if (request.Name.Length is < 3 or > 100)
            errors.Add((nameof(request.Name), "El nombre debe tener entre 3 y 100 caracteres"));

        if (string.IsNullOrWhiteSpace(request.Description))
            errors.Add((nameof(request.Description), "La descripción es obligatoria"));
        else if (request.Description.Length is < 10 or > 100)
            errors.Add((nameof(request.Description), "La descripción debe tener entre 10 y 100 caracteres"));

        if (errors.Count > 0)
            throw new ValidationException().WithDetail(errors);
    }

    private static void ValidateNameFilter(string? name)
    {
        if (!string.IsNullOrWhiteSpace(name) && name.Length is < 3 or > 100)
            throw new ValidationException().WithDetail(nameof(name), "El nombre debe tener entre 3 y 100 caracteres");
    }

    private static SpecialityModel.Response ToResponse(Speciality speciality) =>
        new(speciality.Id, speciality.Name, speciality.Description);
}