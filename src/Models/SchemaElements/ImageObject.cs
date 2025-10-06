using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Models.SchemaElements;

public class ImageObject : SchemaEntity
{
    [JsonPropertyName("@type")]
    public string Type => "ImageObject";
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}