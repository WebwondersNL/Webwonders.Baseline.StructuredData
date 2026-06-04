using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;
using Webwonders.Baseline.StructuredData.Models.Old;

namespace Webwonders.Baseline.StructuredData.Models.Schemas;

public class FAQPage : SchemaEntity
{
    [JsonPropertyName("@context")]
    public string Context => Constants.SchemaData.SchemaOrgContext;

    [JsonPropertyName("@type")]
    public string Type => "FAQPage";

    [JsonPropertyName("mainEntity")]
    public List<Question>? MainEntity { get; set; }
}
