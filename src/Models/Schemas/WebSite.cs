using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;
using Webwonders.Baseline.StructuredData.Models.SchemaElements;

namespace Webwonders.Baseline.StructuredData.Models.Schemas;

public class WebSite : SchemaEntity
{
    [JsonPropertyName("@context")]
    public string Context => Constants.SchemaData.SchemaOrgContext;

    [JsonPropertyName("@type")]
    public string Type => "WebSite";

    [JsonPropertyName("@id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("potentialAction")]
    public SearchAction? PotentialAction { get; set; }

    [JsonPropertyName("publisher")]
    public Organization? Publisher { get; set; }

    [JsonPropertyName("inLanguage")]
    public string? InLanguage { get; set; }
}
