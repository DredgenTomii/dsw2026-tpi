using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentModel.Response> Create(AppointmentModel.Request request, string email);
    Task<IEnumerable<AppointmentModel.Response>> GetByPatient(long dni, string email);
    Task Cancel(Guid id, string email);

    Task<IEnumerable<AppointmentModel.Response>> GetByDate(DateOnly date);

    Task<Pagination<AppointmentModel.Response>> Search(
        int pageSize,
        int pageIndex,
        Guid? specialtyId = null,
        Guid? doctorId = null,
        long? dni = null,
        DateOnly? date = null);
}
