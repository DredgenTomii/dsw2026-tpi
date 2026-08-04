using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
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

    public async Task<IEnumerable<AvailabilityModel.SlotResponse>> GetByDoctor(Guid doctorId, DateOnly? from, DateOnly? to)
    {
        _ = await _persistence.GetById<Doctor>(doctorId) ?? throw new EntityNotFoundException(nameof(Doctor));

        var slots = await _persistence.GetFiltered<AvailabilitySlot>(s =>
            s.DoctorId == doctorId &&
            (from == null || s.Date >= from) &&
            (to == null || s.Date <= to));

        return (slots ?? [])
            .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
            .Select(ToSlotResponse);
    }

    private async Task<AvailabilityModel.Response> GenerateMonth(AvailabilityModel.Request request, bool isOverwrite)
    {
        var doctor = await _persistence.GetById<Doctor>(request.DoctorId)
            ?? throw new EntityNotFoundException(nameof(Doctor));

        if (!doctor.IsActive)
            throw new BusinessRuleException("El médico no se encuentra activo", "DOCTOR_INACTIVE");

        ValidateMonth(request.Year, request.Month);

        var rules = request.Rules?.ToList() ?? [];
        if (rules.Count == 0)
            throw new ValidationException("Debe indicar al menos un día de disponibilidad", "AVAILABILITY_EMPTY_RULES");

        ValidateBlocksOf30(rules);   // RN02: bloques de 30 min
        ValidateNoOverlaps(rules);   // RN01: disponibilidad médica sin solapamientos

        await HandleExistingMonth(request, isOverwrite);

        var (rangeStart, lastOfMonth) = ResolveGenerationRange(request.Year, request.Month);
        var holidays = await _holidayProvider.GetHolidays(request.Year, request.Month);

        var createdSlots = new List<AvailabilitySlot>();

        foreach (var dayRule in rules)
        {
            var rule = new AvailabilityRule(request.DoctorId, request.Year, request.Month,
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
            request.Year,
            request.Month,
            createdSlots.OrderBy(s => s.Date).ThenBy(s => s.StartTime).Select(ToSlotResponse));
    }

    private async Task HandleExistingMonth(
        AvailabilityModel.Request request,
        bool isOverwrite)
    {
        var existingRules = (await _persistence.GetFiltered<AvailabilityRule>(r =>
            r.DoctorId == request.DoctorId &&
            r.Year == request.Year &&
            r.Month == request.Month))?.ToList() ?? [];

        if (existingRules.Count == 0)
            return;

        if (!isOverwrite)
            throw new ConflictException(
                "AVAILABILITY_ALREADY_EXISTS",
                "Ya existe una configuración de disponibilidad para ese mes. Utilice PUT para reemplazarla.");

        var existingRuleIds = existingRules
            .Select(r => r.Id)
            .ToHashSet();

        var existingSlots = (await _persistence.GetFiltered<AvailabilitySlot>(s =>
            existingRuleIds.Contains(s.AvailabilityRuleId)))?.ToList() ?? [];

        foreach (var slot in existingSlots.Where(s => !s.IsBooked))
        {
            await _persistence.Delete(slot);
        }

        var bookedRuleIds = existingSlots
            .Where(s => s.IsBooked)
            .Select(s => s.AvailabilityRuleId)
            .ToHashSet();

        foreach (var rule in existingRules)
        {
            if (!bookedRuleIds.Contains(rule.Id))
            {
                await _persistence.Delete(rule);
            }
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

    private static void ValidateMonth(int year, int month)
    {
        if (month is < 1 or > 12)
            throw new ValidationException("El mes debe estar entre 1 y 12", "AVAILABILITY_INVALID_MONTH");

        if (year < DateTime.UtcNow.Year)
            throw new ValidationException("El año indicado no es válido", "AVAILABILITY_INVALID_YEAR");
    }

    private static void ValidateBlocksOf30(List<AvailabilityModel.DayRuleRequest> rules)
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

    private static void ValidateNoOverlaps(List<AvailabilityModel.DayRuleRequest> rules)
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
