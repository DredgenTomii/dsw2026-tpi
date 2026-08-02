namespace Dsw2026Tpi.Domain.Entities;

/// <summary>
/// Bloque concreto de 30 minutos, en una fecha puntual, para un médico.
/// Es lo que Persona 4 va a reservar desde Appointment (IsBooked / MarkBooked / Release).
/// </summary>
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

    /// <summary>Lo va a usar Persona 4 al reservar un turno.</summary>
    public void MarkBooked()
    {
        if (IsBooked) throw new InvalidOperationException("El slot ya está reservado");
        IsBooked = true;
    }

    /// <summary>Lo va a usar Persona 4 al cancelar un turno.</summary>
    public void Release()
    {
        IsBooked = false;
    }
}
