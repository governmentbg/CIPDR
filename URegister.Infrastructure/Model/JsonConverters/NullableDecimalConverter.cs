using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace URegister.Infrastructure.Model.JsonConverters
{
    public class NullableDecimalConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string? stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return null;
                }

                // Try parsing with invariant culture first (expecting '.')
                if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                {
                    return result;
                }

                // Fallback to current culture (to handle ',' as decimal separator if needed)
                if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
                {
                    return result;
                }

                throw new JsonException($"Invalid decimal format: {stringValue}");
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                // Read number directly as decimal if possible
                if (reader.TryGetDecimal(out var numberValue))
                {
                    return numberValue;
                }
                else if (reader.TryGetDouble(out var doubleValue))
                {
                    return (decimal)doubleValue;
                }
            }

            throw new JsonException("Unexpected token type when parsing decimal.");
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }


}
