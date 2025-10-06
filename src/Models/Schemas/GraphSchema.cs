using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Models.Schemas;

public class GraphSchema
{
    [JsonPropertyName("@context")]
    public string Context { get; set; } = Constants.SchemaData.SchemaOrgContext;

    [JsonPropertyName("@graph")]
    public List<SchemaEntity> Graph { get; set; } = new();
}