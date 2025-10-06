using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Models.SchemaElements;

public class Rating : SchemaEntity
{
    [JsonPropertyName("@type")]
    public string Type => "Rating";

    [JsonPropertyName("ratingValue")]
    public double? RatingValue { get; set; }

    [JsonPropertyName("bestRating")]
    public double? BestRating { get; set; }
    
}