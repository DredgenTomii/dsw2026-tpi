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
using Microsoft.Extensions.Logging;

namespace Dsw2026Tpi.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IPersistence _persistence;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(IPersistence persistence, ILogger<AppointmentService> logger)
    {
        _persistence = persistence;
        _logger = logger;
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
            _logger.LogWarning("Conflicto de concurrencia al reservar el slot {SlotId}", slot.Id);
            throw new ConflictException(ErrorCodes.APPOINTMENT_CONFLICT,
                nameof(ErrorCodes.APPOINTMENT_CONFLICT));
        }

        slot.MarkBooked();
        await _persistence.Update(slot);

        _logger.LogInformation(
            "Turno {AppointmentId} reservado para el paciente {PatientId} en el slot {SlotId}",
            appointment.Id, patient.Id, slot.Id);

        return MapToResponse(appointment, slot, patient);
    }

    private static AppointmentModel.Response MapToResponse(
        Appointment appointment, AvailabilitySlot slot, Patient patient)
    {
        return new AppointmentModel.Response(
            appointment.Id,
            AppointmentStatusMapper.ToApi(appointment.Status),
            new AppointmentModel.PatientResponse(patient.Dni, patient.Name ?? string.Empty),
            new AppointmentModel.DoctorResponse(
                slot.DoctorId,
                slot.Doctor?.Name ?? string.Empty,
                new AppointmentModel.SpecialityResponse(
                    slot.Doctor?.Speciality?.Id ?? Guid.Empty,
                    slot.Doctor?.Speciality?.Name ?? string.Empty)));
    }

    public async Task<IEnumerable<AppointmentModel.Response>> GetByPatient(long dni)
    {
        var patient = await _persistence.First<Patient>(p => p.Dni == dni)
            ?? throw new EntityNotFoundException(nameof(Patient));

        var appointments = await _persistence.GetFiltered<Appointment>(
            a => a.PatientId == patient.Id && a.Status == AppointmentStatus.Booked,
            "AvailabilitySlot.Doctor.Speciality") ?? [];

        return appointments
            .OrderBy(a => a.AvailabilitySlot!.Date)
            .ThenBy(a => a.AvailabilitySlot!.StartTime)
            .Select(a => MapToResponse(a, a.AvailabilitySlot!, patient))
            .ToList();
    }

    public async Task Cancel(Guid id)
    {
        var appointment = await _persistence.GetById<Appointment>(id, "AvailabilitySlot")
            ?? throw new EntityNotFoundException(nameof(Appointment));

        if (appointment.Status != AppointmentStatus.Booked)
            throw new ConflictException(ErrorCodes.APPOINTMENT_NOT_CANCELLABLE,
                nameof(ErrorCodes.APPOINTMENT_NOT_CANCELLABLE));

        appointment.Cancel();
        await _persistence.Update(appointment);

        if (appointment.AvailabilitySlot is not null)
        {
            appointment.AvailabilitySlot.Release();
            await _persistence.Update(appointment.AvailabilitySlot);
        }

        _logger.LogInformation("Turno {AppointmentId} cancelado", appointment.Id);
    }

    public async Task<IEnumerable<AppointmentModel.Response>> GetByDate(DateOnly date)
    {
        var appointments = await _persistence.GetFiltered<Appointment>(
            a => a.AvailabilitySlot!.Date == date,
            "AvailabilitySlot.Doctor.Speciality", "Patient") ?? [];

        return appointments
            .OrderBy(a => a.AvailabilitySlot!.StartTime)
            .Select(a => MapToResponse(a, a.AvailabilitySlot!, a.Patient!))
            .ToList();
    }

    public async Task<Pagination<AppointmentModel.Response>> Search(
        int pageSize,
        int pageIndex,
        Guid? specialtyId = null,
        Guid? doctorId = null,
        long? dni = null,
        DateOnly? date = null)
    {
        if (pageSize <= 0) pageSize = 10;
        if (pageIndex < 0) pageIndex = 0;

        var page = await _persistence.Paginate<Appointment, DateOnly>(
            pageSize,
            pageIndex,
            a => (specialtyId == null || a.AvailabilitySlot!.Doctor!.SpecialityId == specialtyId)
              && (doctorId == null || a.AvailabilitySlot!.DoctorId == doctorId)
              && (dni == null || a.Patient!.Dni == dni)
              && (date == null || a.AvailabilitySlot!.Date == date),
            a => a.AvailabilitySlot!.Date,
            "AvailabilitySlot.Doctor.Speciality", "Patient");

        return page.Map(a => MapToResponse(a, a.AvailabilitySlot!, a.Patient!));
    }
}
