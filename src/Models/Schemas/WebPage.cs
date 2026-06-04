using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Models.Schemas;

public class WebPage : SchemaEntity
{
    [JsonPropertyName("@context")]
    public string Context => Constants.SchemaData.SchemaOrgContext;

    [JsonPropertyName("@type")]
    public string Type => "WebPage";

    [JsonPropertyName("@id")]
    public string? Id { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("inLanguage")]
    public string? InLanguage { get; set; }

    [JsonPropertyName("breadcrumb")]
    public BreadcrumbList? Breadcrumb { get; set; }

    [JsonPropertyName("publisher")]
    public Organization? Publisher { get; set; }

    [JsonPropertyName("isPartOf")]
    public WebSite? IsPartOf { get; set; }
}
