using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dsw2026Tpi.Api.Json;

public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
	private const string SerializationFormat = "HH:mm";

	private static readonly string[] AcceptedFormats = ["HH:mm", "HH:mm:ss"];

	public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();

		if (string.IsNullOrWhiteSpace(value))
			throw new JsonException("Se esperaba una hora en formato \"HH:mm\".");

		if (TimeOnly.TryParseExact(value, AcceptedFormats, CultureInfo.InvariantCulture,
				System.Globalization.DateTimeStyles.None, out var result))
		{
			return result;
		}

		throw new JsonException($"Formato de hora inválido: \"{value}\". Se esperaba \"HH:mm\".");
	}

	public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString(SerializationFormat, CultureInfo.InvariantCulture));
	}
}