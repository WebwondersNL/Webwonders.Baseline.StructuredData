using System.Text.Json;
using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Converters;

public class SchemaEntityConverter : JsonConverter<SchemaEntity>
{
    public override SchemaEntity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotSupportedException("Deserialization not implemented for SchemaEntity");

    public override void Write(Utf8JsonWriter writer, SchemaEntity value, JsonSerializerOptions options)
    {
        var type = value.GetType();
        JsonSerializer.Serialize(writer, value, type, options);
    }
}