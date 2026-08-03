using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces;

public interface ISpecialityService
{
    Task<Pagination<SpecialityModel.Response>>;
    Task<SpecialityModel.Response> Add(SpecialityModel.Request request);
    Task<SpecialityModel.Response> Update(Guid id, SpecialityModel.Request request);
    Task Delete(Guid id);
}