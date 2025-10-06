using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Models.SchemaElements;

public class Place : SchemaEntity
{
    [JsonPropertyName("@type")]
    public string Type => "Place";
    
    [JsonPropertyName("@id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("address")]
    public PostalAddress? Address { get; set; }
    
    [JsonPropertyName("openingHoursSpecification")]
    public List<OpeningHoursSpecification>? OpeningHoursSpecification { get; set; }
}