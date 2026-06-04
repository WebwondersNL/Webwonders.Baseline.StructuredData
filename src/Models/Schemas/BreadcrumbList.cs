using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;
using Webwonders.Baseline.StructuredData.Models.SchemaElements;

namespace Webwonders.Baseline.StructuredData.Models.Schemas;

public class BreadcrumbList : SchemaEntity
{
    [JsonPropertyName("@context")]
    public string Context => Constants.SchemaData.SchemaOrgContext;

    [JsonPropertyName("@type")]
    public string Type => "BreadcrumbList";

    [JsonPropertyName("itemListElement")]
    public List<ItemListElement>? ItemListElement { get; set; }
}
