using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Models.SchemaElements;

public class PostalAddress : SchemaEntity
{
    [JsonPropertyName("@type")]
    public string Type => "PostalAddress";
    
    [JsonPropertyName("streetAddress")]
    public string? StreetAddress { get; set; }
    
    [JsonPropertyName("addressLocality")]
    public string? AddressLocality { get; set; }
    
    [JsonPropertyName("postalCode")]
    public string? PostalCode { get; set; }
    
    [JsonPropertyName("addressCountry")]
    public string? AddressCountry { get; set; }
}