using System.Text.Json.Serialization;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Models.SchemaElements;

public class LocalBusiness : SchemaEntity
{
        [JsonPropertyName("@type")]
        public string Type => "LocalBusiness";
        
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("image")]
        public ImageObject? Image { get; set; }
        
        [JsonPropertyName("address")]
        public PostalAddress? Address { get; set; }
        
        [JsonPropertyName("telephone")]
        public string? Telephone { get; set; }
        
        [JsonPropertyName("openingHoursSpecification")]
        public IEnumerable<OpeningHoursSpecification>? OpeningHours { get; set; }
        
        [JsonPropertyName("geo")]
        public GeoCoordinates? Geo { get; set; }
}