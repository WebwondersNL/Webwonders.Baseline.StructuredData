using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Models.SchemaElements;

public class SearchAction : SchemaEntity
{
    [JsonPropertyName("@type")]
    public string Type => "SearchAction";

    [JsonPropertyName("target")]
    public string? Target { get; set; } = string.Empty;

    [JsonPropertyName("query-input")]
    public string? QueryInput { get; set; } = "required name=search_term_string";
}