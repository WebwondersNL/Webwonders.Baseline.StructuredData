using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Models.SchemaElements;

public class OpeningHoursSpecification : SchemaEntity
{
    [JsonPropertyName("@type")]
    public string Type => "OpeningHoursSpecification";
    
    [JsonPropertyName("dayOfWeek")]
    public string? DayOfWeek { get; set; } = string.Empty;
    
    [JsonPropertyName("opens")]
    public string? Opens { get; set; } = string.Empty;
    
    [JsonPropertyName("closes")]
    public string? Closes { get; set; } = string.Empty;
}