using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeslaApi.Converters
{
    public class VehicleStateConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString() == "online";
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteString("", value ? "online" : "offline");
        }
    }
}