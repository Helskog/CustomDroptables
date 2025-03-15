using Stunlock.Core;
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CustomDropTables.Config
{
    public class JsonStringToIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String &&
                int.TryParse(reader.GetString(), out int valueFromString))
            {
                return valueFromString;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }

            throw new JsonException("Value is not an integer or numeric string.");
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options) => writer.WriteNumberValue(value);
    }

    public class JsonStringToFloatConverter : JsonConverter<float>
    {
        public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string stringValue = reader.GetString();
                if (float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                {
                    return value;
                }
                throw new JsonException($"Unable to convert \"{stringValue}\" to a float.");
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetSingle();
            }

            throw new JsonException("Unexpected token type when parsing float.");
        }

        public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options) => writer.WriteNumberValue(value);
    }

    public class JsonStringToPrefabGUIDConverter : JsonConverter<PrefabGUID>
    {
        public override PrefabGUID Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String &&
                int.TryParse(reader.GetString(), out int valueFromString))
            {
                return new PrefabGUID(valueFromString);
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return new PrefabGUID(reader.GetInt32());
            }

            throw new JsonException("Value is not an integer or numeric string.");
        }

        public override void Write(Utf8JsonWriter writer, PrefabGUID value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
    }

    public class JsonStringToRangeIntConverter : JsonConverter<RangeInt>
    {
        public override RangeInt Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                int value = reader.GetInt32();
                return new RangeInt { Min = value, Max = value };
            }

            else if (reader.TokenType == JsonTokenType.String)
            {
                string str = reader.GetString();
                if (str.Contains("-"))
                {
                    var parts = str.Split('-', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int min) &&
                        int.TryParse(parts[1], out int max))
                    {
                        return new RangeInt { Min = min, Max = max };
                    }
                }
                else if (int.TryParse(str, out int single))
                {
                    return new RangeInt { Min = single, Max = single };
                }
            }
            throw new JsonException("Invalid format for RangeInt.");
        }

        public override void Write(Utf8JsonWriter writer, RangeInt value, JsonSerializerOptions options)
        {
            if (!value.IsRange)
            {
                writer.WriteNumberValue(value.Min);
            }
            else
            {
                writer.WriteStringValue($"{value.Min}-{value.Max}");
            }
        }
    }
}
