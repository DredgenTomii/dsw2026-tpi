using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAvailabilityService
{
    Task<AvailabilityModel.Response> Create(AvailabilityModel.Request request);
    Task<AvailabilityModel.Response> Replace(AvailabilityModel.Request request);
    Task<IEnumerable<AvailabilityModel.RuleResponse>> GetByDoctor(Guid doctorId);
}
