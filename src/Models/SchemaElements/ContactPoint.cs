using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Models.SchemaElements;

public class ContactPoint : SchemaEntity
{
    [JsonPropertyName("@type")]
    public string Type => "ContactPoint";
    
    [JsonPropertyName("contactType")]
    public string? ContactType { get; set; }
    
    [JsonPropertyName("telephone")]
    public string? Telephone { get; set; }
    
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("areaServed")] 
    public string? AreaServed { get; set; } = "NL";
}