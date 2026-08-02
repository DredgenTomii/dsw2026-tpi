namespace Dsw2026Tpi.Domain.Interfaces;

public interface IHolidayProvider
{
    /// <summary>Devuelve el conjunto de fechas feriadas/no laborables para un año y mes dado.</summary>
    Task<HashSet<DateOnly>> GetHolidays(int year, int month);
}
