using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Models.SchemaElements;

public class Offer : SchemaEntity
{
    [JsonPropertyName("@type")]
    public string Type => "Offer";
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }
    
    [JsonPropertyName("priceCurrency")]
    public string? PriceCurrency { get; set; }
    
    [JsonPropertyName("priceValidUntil")]
    public string? PriceValidUntil { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
}