using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Presentation.Common;

/// <summary>
/// Custom JSON converter for DateTime formatting with AM/PM
/// Converts DateTime to readable format: "Dec 20, 2024 2:30 PM"
/// </summary>
public class ReadableDateTimeConverter : JsonConverter<DateTime>
{
    private const string DateTimeFormat = "MMM dd, yyyy h:mm tt";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();
        if (string.IsNullOrEmpty(dateString))
        {
            return default;
        }

        // Try parsing multiple formats
        if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
        {
            return result;
        }

        throw new JsonException($"Unable to parse '{dateString}' as DateTime.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Write in readable format with AM/PM
        writer.WriteStringValue(value.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
    }
}

/// <summary>
/// Custom JSON converter for nullable DateTime with AM/PM formatting
/// </summary>
public class ReadableNullableDateTimeConverter : JsonConverter<DateTime?>
{
    private const string DateTimeFormat = "MMM dd, yyyy h:mm tt";

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();
        if (string.IsNullOrEmpty(dateString))
        {
            return null;
        }

        if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
        {
            return result;
        }

        throw new JsonException($"Unable to parse '{dateString}' as DateTime.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString(DateTimeFormat, CultureInfo.InvariantCulture));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
