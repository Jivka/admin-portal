using System.Text.Json;
using System.Text.Json.Serialization;

namespace AP.Common.Utilities.Converters;

public class DecimalToStringWriterConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetDecimal();

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) =>
           writer.WriteStringValue(value.ToString("0.###################")); // precision of 19 digits without trailing zeros
}