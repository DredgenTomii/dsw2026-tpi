namespace Dsw2026Tpi.Domain.Interfaces;

public interface IHolidayProvider
{
    
    Task<HashSet<DateOnly>> GetHolidays(int year, int month);
}
