using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Helpers;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private const int SlotMinutes = 30;

    private readonly IPersistence _persistence;
    private readonly IHolidayProvider _holidayProvider;

    public AvailabilityService(IPersistence persistence, IHolidayProvider holidayProvider)
    {
        _persistence = persistence;
        _holidayProvider = holidayProvider;
    }

    public Task<AvailabilityModel.Response> Create(AvailabilityModel.Request request) =>
        GenerateMonth(request, isOverwrite: false);

    public Task<AvailabilityModel.Response> Replace(AvailabilityModel.Request request) =>
        GenerateMonth(request, isOverwrite: true);

    public async Task<IEnumerable<AvailabilityModel.RuleResponse>> GetByDoctor(Guid doctorId)
    {
        _ = await _persistence.GetById<Doctor>(doctorId) ?? throw new EntityNotFoundException(nameof(Doctor));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var rules = await _persistence.GetFiltered<AvailabilityRule>(r =>
            r.DoctorId == doctorId && r.Year == today.Year && r.Month == today.Month) ?? [];

        return rules
            .OrderBy(r => r.DayOfWeek)
            .Select(r => new AvailabilityModel.RuleResponse(r.Id, r.DayOfWeek.ToSpanish(), r.StartTime, r.EndTime));
    }

    private async Task<AvailabilityModel.Response> GenerateMonth(AvailabilityModel.Request request, bool isOverwrite)
    {
        var doctor = await _persistence.GetById<Doctor>(request.DoctorId)
            ?? throw new EntityNotFoundException(nameof(Doctor));

        if (doctor.Deleted)
            throw new BusinessRuleException("El médico no se encuentra activo", "DOCTOR_INACTIVE");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var year = today.Year;
        var month = today.Month;

        var days = request.Days?.ToList() ?? [];
        if (days.Count == 0)
            throw new ValidationException("Debe indicar al menos un día de disponibilidad", "AVAILABILITY_EMPTY_RULES");

        var rules = days
            .Select(d => (DayOfWeek: d.Day.ToDayOfWeek(), d.StartTime, d.EndTime))
            .ToList();

        ValidateBlocksOf30(rules);   // RN02: bloques de 30 min
        ValidateNoOverlaps(rules);   // RN01: disponibilidad médica sin solapamientos

        await HandleExistingMonth(request.DoctorId, year, month, isOverwrite);

        var (rangeStart, lastOfMonth) = ResolveGenerationRange(year, month);
        var holidays = await _holidayProvider.GetHolidays(year, month);

        var createdSlots = new List<AvailabilitySlot>();

        foreach (var dayRule in rules)
        {
            var rule = new AvailabilityRule(request.DoctorId, year, month,
                dayRule.DayOfWeek, dayRule.StartTime, dayRule.EndTime);
            await _persistence.Add(rule);

            for (var date = rangeStart; date <= lastOfMonth; date = date.AddDays(1))
            {
                if (date.DayOfWeek != dayRule.DayOfWeek) continue;
                if (holidays.Contains(date)) continue;

                foreach (var (start, end) in SplitInSlotsOf30(dayRule.StartTime, dayRule.EndTime))
                {
                    var slot = new AvailabilitySlot(request.DoctorId, rule.Id, date, start, end);
                    await _persistence.Add(slot);
                    createdSlots.Add(slot);
                }
            }
        }

        return new AvailabilityModel.Response(
            request.DoctorId,
            year,
            month,
            createdSlots.OrderBy(s => s.Date).ThenBy(s => s.StartTime).Select(ToSlotResponse));
    }

    /// <summary>
    /// Si ya hay reglas cargadas para ese médico/mes: en POST es conflicto (usar PUT),
    /// en PUT se borran (cascada borra los slots) siempre que ninguno esté reservado.
    /// </summary>
    private async Task HandleExistingMonth(Guid doctorId, int year, int month, bool isOverwrite)
    {
        var existingRules = (await _persistence.GetFiltered<AvailabilityRule>(r =>
            r.DoctorId == doctorId && r.Year == year && r.Month == month))?.ToList() ?? [];

        if (existingRules.Count == 0) return;

        if (!isOverwrite)
            throw new ConflictException("AVAILABILITY_ALREADY_EXISTS",
                "Ya existe una configuración de disponibilidad para ese mes. Utilice PUT para reemplazarla.");

        var existingRuleIds = existingRules.Select(r => r.Id).ToHashSet();
        var existingSlots = await _persistence.GetFiltered<AvailabilitySlot>(s =>
            existingRuleIds.Contains(s.AvailabilityRuleId)) ?? [];

        if (existingSlots.Any(s => s.IsBooked))
            throw new ConflictException("AVAILABILITY_HAS_BOOKED_SLOTS",
                "No se puede sobreescribir el mes: hay turnos reservados. Cancele esos turnos primero.");

        foreach (var rule in existingRules)
        {
            // OnDelete(Cascade) en AvailabilitySlotConfiguration borra los slots asociados.
            await _persistence.Delete(rule);
        }
    }

    private static (DateOnly RangeStart, DateOnly LastOfMonth) ResolveGenerationRange(int year, int month)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var firstOfMonth = new DateOnly(year, month, 1);
        var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);
        var rangeStart = firstOfMonth > today ? firstOfMonth : today;

        if (rangeStart > lastOfMonth)
            throw new ValidationException("El mes indicado ya finalizó", "AVAILABILITY_MONTH_PAST");

        return (rangeStart, lastOfMonth);
    }

    private static IEnumerable<(TimeOnly Start, TimeOnly End)> SplitInSlotsOf30(TimeOnly start, TimeOnly end)
    {
        var current = start;
        while (current.AddMinutes(SlotMinutes) <= end)
        {
            var next = current.AddMinutes(SlotMinutes);
            yield return (current, next);
            current = next;
        }
    }

    /// <summary>RN02: cada franja declarada debe poder dividirse en bloques exactos de 30 min.</summary>
    private static void ValidateBlocksOf30(List<(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime)> rules)
    {
        foreach (var rule in rules)
        {
            if (rule.EndTime <= rule.StartTime)
                throw new BusinessRuleException(
                    $"El horario del día {rule.DayOfWeek} es inválido: el fin debe ser posterior al inicio",
                    "RN02_INVALID_TIME_RANGE");

            var totalMinutes = (rule.EndTime.ToTimeSpan() - rule.StartTime.ToTimeSpan()).TotalMinutes;
            if (totalMinutes % SlotMinutes != 0)
                throw new BusinessRuleException(
                    $"El horario del día {rule.DayOfWeek} debe dividirse en bloques exactos de {SlotMinutes} minutos",
                    "RN02_INVALID_TIME_BLOCK");
        }
    }

    /// <summary>RN01: el médico no puede tener dos franjas que se solapen el mismo día.</summary>
    private static void ValidateNoOverlaps(List<(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime)> rules)
    {
        foreach (var group in rules.GroupBy(r => r.DayOfWeek))
        {
            var ordered = group.OrderBy(r => r.StartTime).ToList();

            for (var i = 1; i < ordered.Count; i++)
            {
                if (ordered[i].StartTime < ordered[i - 1].EndTime)
                    throw new BusinessRuleException(
                        $"Existen horarios superpuestos para el médico el día {group.Key}",
                        "RN01_OVERLAPPING_AVAILABILITY");
            }
        }
    }

    private static AvailabilityModel.SlotResponse ToSlotResponse(AvailabilitySlot slot) =>
        new(slot.Id, slot.Date, slot.StartTime, slot.EndTime, slot.IsBooked);
}
