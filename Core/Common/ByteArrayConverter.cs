using System.Text.Json;

namespace Core.Common
{
    public class ByteArrayConverter : System.Text.Json.Serialization.JsonConverter<byte[]>
    {
        public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var byteList = JsonSerializer.Deserialize<List<byte>>(ref reader, options);
            return byteList?.ToArray();
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.ToList(), options);
        }
    }
}
