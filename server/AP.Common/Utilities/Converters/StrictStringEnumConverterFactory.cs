using System.Text.Json;
using System.Text.Json.Serialization;

namespace AP.Common.Utilities.Converters;

public class StrictStringEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum || Nullable.GetUnderlyingType(typeToConvert)?.IsEnum == true;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type enumType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
        Type converterType = (Nullable.GetUnderlyingType(typeToConvert) != null)
            ? typeof(NullableStrictStringEnumConverter<>).MakeGenericType(enumType)
            : typeof(StrictStringEnumConverter<>).MakeGenericType(enumType);

        return (JsonConverter)(Activator.CreateInstance(converterType)
            ?? throw new InvalidOperationException($"Unable to create an instance of {converterType}."));
    }
}

public class StrictStringEnumConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string token for {typeToConvert.Name}.");
        }

        var enumString = reader.GetString();
        if (Enum.TryParse<T>(enumString, true, out var result))
        {
            return result;
        }

        throw new JsonException($"Unable to convert \"{enumString}\" to {typeToConvert.Name}.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

public class NullableStrictStringEnumConverter<T> : JsonConverter<T?>
    where T : struct, Enum
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string token for {typeToConvert.Name} enum.");
        }

        var enumString = reader.GetString();
        if (Enum.TryParse<T>(enumString, true, out var result))
        {
            return result;
        }

        throw new JsonException($"Unable to convert \"{enumString}\" to {typeToConvert.Name}.");
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString());
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}