namespace Dsw2026Tpi.CrossCutting.Helpers;

public static class DayOfWeekEsExtensions
{
    private static readonly Dictionary<string, DayOfWeek> ToEnum = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LUNES"] = DayOfWeek.Monday,
        ["MARTES"] = DayOfWeek.Tuesday,
        ["MIERCOLES"] = DayOfWeek.Wednesday,
        ["MIÉRCOLES"] = DayOfWeek.Wednesday,
        ["JUEVES"] = DayOfWeek.Thursday,
        ["VIERNES"] = DayOfWeek.Friday,
        ["SABADO"] = DayOfWeek.Saturday,
        ["SÁBADO"] = DayOfWeek.Saturday,
        ["DOMINGO"] = DayOfWeek.Sunday,
    };

    private static readonly Dictionary<DayOfWeek, string> ToText = new()
    {
        [DayOfWeek.Monday] = "LUNES",
        [DayOfWeek.Tuesday] = "MARTES",
        [DayOfWeek.Wednesday] = "MIÉRCOLES",
        [DayOfWeek.Thursday] = "JUEVES",
        [DayOfWeek.Friday] = "VIERNES",
        [DayOfWeek.Saturday] = "SÁBADO",
        [DayOfWeek.Sunday] = "DOMINGO",
    };

    public static DayOfWeek ToDayOfWeek(this string day)
    {
        if (!ToEnum.TryGetValue(day.Trim(), out var result))
            throw new ArgumentException($"Día inválido: {day}");
        return result;
    }

    public static string ToSpanish(this DayOfWeek day) => ToText[day];
}
