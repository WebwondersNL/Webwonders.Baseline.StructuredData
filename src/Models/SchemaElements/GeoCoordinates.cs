using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Models.SchemaElements;

public class GeoCoordinates : SchemaEntity
{
    [JsonPropertyName("@type")]
    public string Type => "GeoCoordinates";
    
    [JsonPropertyName("latitude")]
    public string? Latitude { get; set; }
    
    [JsonPropertyName("longitude")]
    public string? Longitude { get; set; }
}