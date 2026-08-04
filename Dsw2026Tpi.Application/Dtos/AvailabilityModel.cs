namespace Dsw2026Tpi.Application.Dtos;

public record AvailabilityModel
{
    public record DayRuleRequest(string Day, TimeOnly StartTime, TimeOnly EndTime);

    public record Request(Guid DoctorId, IEnumerable<DayRuleRequest> Days);

    public record SlotResponse(Guid Id, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, bool IsBooked);

    public record Response(Guid DoctorId, int Year, int Month, IEnumerable<SlotResponse> Slots);

    public record RuleResponse(Guid Id, string Day, TimeOnly StartTime, TimeOnly EndTime);
}
