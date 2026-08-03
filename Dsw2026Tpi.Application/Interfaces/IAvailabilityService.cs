using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAvailabilityService
{
    /// <summary>POST: crea la disponibilidad del mes. Falla si ya existía una configuración.</summary>
    Task<AvailabilityModel.Response> Create(AvailabilityModel.Request request);

    /// <summary>PUT: sobreescribe toda la disponibilidad del mes para ese médico.</summary>
    Task<AvailabilityModel.Response> Replace(AvailabilityModel.Request request);

    /// <summary>Usado por GET /api/doctors/{id}/availabilities.</summary>
    Task<IEnumerable<AvailabilityModel.SlotResponse>> GetByDoctor(Guid doctorId, DateOnly? from, DateOnly? to);
}
