namespace Dsw2026Tpi.Domain.Entities;


public class AvailabilitySlot : EntityBase
{
    public Guid DoctorId { get; init; }
    public Doctor? Doctor { get; private set; }
    public Guid AvailabilityRuleId { get; init; }
    public AvailabilityRule? AvailabilityRule { get; private set; }
    public DateOnly Date { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public bool IsBooked { get; private set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private AvailabilitySlot()
    {
    }
#pragma warning restore CS8618
    #endregion

    public AvailabilitySlot(
        Guid doctorId,
        Guid availabilityRuleId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? id = null) : base(id)
    {
        DoctorId = doctorId;
        AvailabilityRuleId = availabilityRuleId;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        IsBooked = false;
    }

    
    public void MarkBooked()
    {
        if (IsBooked) throw new InvalidOperationException("El slot ya está reservado");
        IsBooked = true;
    }

    
    public void Release()
    {
        IsBooked = false;
    }
}
