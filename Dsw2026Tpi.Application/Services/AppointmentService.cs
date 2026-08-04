using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dsw2026Tpi.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IPersistence _persistence;

    public AppointmentService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<AppointmentModel.Response> Create(AppointmentModel.Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length < 5)
            throw new ValidationException(ErrorCodes.APPOINTMENT_INVALID_REASON,
                nameof(ErrorCodes.APPOINTMENT_INVALID_REASON));

        var dniLength = request.Patient.Dni.ToString().Length;
        if (dniLength < 7 || dniLength > 10)
            throw new ValidationException(ErrorCodes.APPOINTMENT_INVALID_DNI,
                nameof(ErrorCodes.APPOINTMENT_INVALID_DNI));

        var patient = await _persistence.First<Patient>(p => p.Dni == request.Patient.Dni)
            ?? throw new EntityNotFoundException(nameof(Patient));

        var slot = await _persistence.GetById<AvailabilitySlot>(
            request.AvailabilitySlotId, "Doctor.Speciality")
            ?? throw new EntityNotFoundException(nameof(AvailabilitySlot));

        if (slot.DoctorId != request.DoctorId)
            throw new ValidationException(ErrorCodes.APPOINTMENT_DOCTOR_MISMATCH,
                nameof(ErrorCodes.APPOINTMENT_DOCTOR_MISMATCH));

        if (slot.Date.ToDateTime(slot.StartTime) <= DateTime.Now)
            throw new ValidationException(ErrorCodes.APPOINTMENT_PAST_DATE,
                nameof(ErrorCodes.APPOINTMENT_PAST_DATE));

        if (slot.IsBooked)
            throw new ConflictException(ErrorCodes.APPOINTMENT_CONFLICT,
                nameof(ErrorCodes.APPOINTMENT_CONFLICT));

        var appointment = new Appointment(slot.Id, patient.Id, request.Reason.Trim());

        try
        {
            await _persistence.Add(appointment);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException(ErrorCodes.APPOINTMENT_CONFLICT,
                nameof(ErrorCodes.APPOINTMENT_CONFLICT));
        }

        slot.MarkBooked();
        await _persistence.Update(slot);

        return MapToResponse(appointment, slot, patient);
    }

    private static AppointmentModel.Response MapToResponse(
        Appointment appointment, AvailabilitySlot slot, Patient patient)
    {
        return new AppointmentModel.Response(
            appointment.Id,
            appointment.Status.ToString().ToUpperInvariant(),
            new AppointmentModel.PatientResponse(patient.Dni, patient.Name ?? string.Empty),
            new AppointmentModel.DoctorResponse(
                slot.DoctorId,
                slot.Doctor?.Name ?? string.Empty,
                new AppointmentModel.SpecialityResponse(
                    slot.Doctor?.Speciality?.Id ?? Guid.Empty,
                    slot.Doctor?.Speciality?.Name ?? string.Empty)));
    }

    public Task<IEnumerable<AppointmentModel.Response>> GetByPatient(long dni)
    {
        throw new NotImplementedException();
    }

    public Task Cancel(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<AppointmentModel.Response>> GetByDate(DateOnly date)
    {
        throw new NotImplementedException();
    }

    public Task<Pagination<AppointmentModel.Response>> Search(
        int pageSize,
        int pageIndex,
        Guid? specialtyId = null,
        Guid? doctorId = null,
        long? dni = null,
        DateOnly? date = null)
    {
        throw new NotImplementedException();
    }
}
