using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;
using Webwonders.Baseline.StructuredData.Models.SchemaElements;

namespace Webwonders.Baseline.StructuredData.Models.Schemas;

public class Organization : SchemaEntity
{
    [JsonPropertyName("@context")]
    public string Context => Constants.SchemaData.SchemaOrgContext;

    [JsonPropertyName("@type")]
    public string Type => "Organization";

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("logo")]
    public ImageObject? Logo { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("telephone")]
    public string? Telephone { get; set; }

    [JsonPropertyName("address")]
    public PostalAddress? Address { get; set; }

    [JsonPropertyName("sameAs")]
    public string[]? SameAs { get; set; }
}
