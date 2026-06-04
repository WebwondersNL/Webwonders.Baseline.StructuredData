using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Models.SchemaElements;

public class AggregateRating : SchemaEntity
{
    [JsonPropertyName("@type")]
    public string Type => "AggregateRating";

    [JsonPropertyName("ratingValue")]
    public double? RatingValue { get; set; }

    [JsonPropertyName("bestRating")]
    public double? BestRating { get; set; }

    [JsonPropertyName("worstRating")]
    public double? WorstRating { get; set; }

    [JsonPropertyName("reviewCount")]
    public int? ReviewCount { get; set; }
}
