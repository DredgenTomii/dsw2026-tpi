using Dsw2026Tpi.Domain.Interfaces;
using System.Text.Json;

namespace Dsw2026Tpi.Data.Holidays;

public class HolidayProvider : IHolidayProvider
{
    private const string SourceFile = "Sources/holidays.json";
    private static IReadOnlySet<DateOnly>? _cache;
    private static readonly object Lock = new();

    public Task<HashSet<DateOnly>> GetHolidays(int year, int month)
    {
        var all = LoadAll();
        var filtered = all.Where(d => d.Year == year && d.Month == month).ToHashSet();
        return Task.FromResult(filtered);
    }

    private static IReadOnlySet<DateOnly> LoadAll()
    {
        if (_cache is not null) return _cache;

        lock (Lock)
        {
            if (_cache is not null) return _cache;

            var path = Path.Combine(AppContext.BaseDirectory, SourceFile);

            if (!File.Exists(path))
            {
                _cache = new HashSet<DateOnly>();
                return _cache;
            }

            var json = File.ReadAllText(path);
            var dates = JsonSerializer.Deserialize<List<DateOnly>>(json) ?? [];
            _cache = dates.ToHashSet();
            return _cache;
        }
    }
}
