using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Models.SchemaElements;

public class ItemListElement : SchemaEntity
{
    [JsonPropertyName("@type")]
    public string Type => "ListItem";
    
    [JsonPropertyName("position")]
    public int Position { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("item")]
    public string? Item { get; set; }
}