using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;
using Webwonders.Baseline.StructuredData.Models.Schemas;

namespace Webwonders.Baseline.StructuredData.Models.SchemaElements;

public class Person : SchemaEntity
{
    [JsonPropertyName("@type")]
    public string Type => "Person";
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    
    [JsonPropertyName("image")]
    public ImageObject? Image { get; set; }
    
    [JsonPropertyName("sameAs")]
    public string? SameAs { get; set; }
    
    [JsonPropertyName("jobTitle")]
    public string? JobTitle { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("worksFor")]
    public Organization? WorksFor { get; set; }
}