namespace Dsw2026Tpi.Application.Dtos;

public record AvailabilityModel
{
    /// <summary>
    /// Un día de la semana + franja horaria. DayOfWeek se serializa como número
    /// (0=domingo ... 6=sábado, valores del enum System.DayOfWeek).
    /// </summary>
    public record DayRuleRequest(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);

    /// <summary>Body para POST y PUT /api/availabilities.</summary>
    public record Request(Guid DoctorId, int Year, int Month, IEnumerable<DayRuleRequest> Rules);

    public record SlotResponse(Guid Id, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, bool IsBooked);

    public record Response(Guid DoctorId, int Year, int Month, IEnumerable<SlotResponse> Slots);
}
