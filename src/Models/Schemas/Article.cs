using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Models.Schemas;

public class BlogPosting : SchemaEntity
{
    [JsonPropertyName("@context")]
    public string Context => Constants.SchemaData.SchemaOrgContext;

    [JsonPropertyName("@type")]
    public string Type => "BlogPosting";

    [JsonPropertyName("headline")]
    public string? Headline { get; set; }

    [JsonPropertyName("datePublished")]
    public string? DatePublished { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("author")]
    public SchemaEntity? Author { get; set; }

    [JsonPropertyName("publisher")]
    public SchemaEntity? Publisher { get; set; }
}

public class NewsArticle : SchemaEntity
{
    [JsonPropertyName("@context")]
    public string Context => Constants.SchemaData.SchemaOrgContext;

    [JsonPropertyName("@type")]
    public string Type => "NewsArticle";

    [JsonPropertyName("headline")]
    public string? Headline { get; set; }

    [JsonPropertyName("datePublished")]
    public string? DatePublished { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("author")]
    public SchemaEntity? Author { get; set; }

    [JsonPropertyName("publisher")]
    public SchemaEntity? Publisher { get; set; }
}
