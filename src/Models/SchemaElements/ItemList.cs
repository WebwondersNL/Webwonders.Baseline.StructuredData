using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Models.SchemaElements;

public class ItemList : SchemaEntity
{
    [JsonPropertyName("@type")]
    public string Type => "ItemList";
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("itemListElement")]
    public List<LocalBusiness?>? ItemListElement { get; set; }
}