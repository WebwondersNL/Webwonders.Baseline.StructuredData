using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Strings;

namespace Webwonders.Baseline.StructuredData.Models.Old;


public class Question
{
    [JsonPropertyName("@type")]
    public string Type { get; set; } = "Question";
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("acceptedAnswer")]
    public Answer? AcceptedAnswer { get; set; }
}

public class Answer
{
    [JsonPropertyName("@type")]
    public string? Type { get; set; } = "Answer";

    private IHtmlEncodedString? _text;

    [JsonIgnore]
    public IHtmlEncodedString? Text
    {
        get => _text;
        set
        {
            _text = value;
            SerializableText = value?.ToString();
        }
    }

    [JsonPropertyName("text")]
    public string? SerializableText { get; private set; }
}