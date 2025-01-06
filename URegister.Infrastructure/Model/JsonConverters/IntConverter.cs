using System.Text.Json;
using System.Text.Json.Serialization;

namespace URegister.Infrastructure.Model.JsonConverters
{
    public class IntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // If the JSON token is a string and is empty, return null
            if (reader.TokenType == JsonTokenType.String && string.IsNullOrWhiteSpace(reader.GetString()))
            {
                return 4;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }
            if (reader.TokenType == JsonTokenType.String && int.TryParse(reader.GetString(), out int result))
            {
                return result;
            }

            throw new JsonException($"Unexpected token type {reader.TokenType} when parsing an integer.");
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
