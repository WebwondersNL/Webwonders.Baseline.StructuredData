using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Models.SchemaElements;

public class Review : SchemaEntity
{
    [JsonPropertyName("@type")]
    public string Type => "Review";
    
    [JsonPropertyName("author")]
    public Person? Author { get; set; }
    
    [JsonPropertyName("datePublished")]
    public string? DatePublished { get; set; }
    
    [JsonPropertyName("reviewBody")]
    public string? ReviewBody { get; set; }
    
    [JsonPropertyName("reviewRating")]
    public Rating? ReviewRating { get; set; }
}