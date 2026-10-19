using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using Webwonders.Baseline.StructuredData.Abstractions;
using Webwonders.Baseline.StructuredData.Interfaces;
using Webwonders.Baseline.StructuredData.Models.Old;

namespace Webwonders.Baseline.StructuredData.Services;

[Obsolete("Use IStructuredDataRenderer. This service will be removed in the next major version.")]
public class StructuredDataService(IStructuredDataRenderer renderer, IUmbracoContextAccessor umbracoContextAccessor)
    : IStructuredDataService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        // Escapes <, > and & so content can't close the surrounding script tag.
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<string> BuildSchemaAsync(IPublishedContent content)
        => await renderer.RenderJsonAsync(content) ?? string.Empty;

    public async Task<string> BuildSchemaAsync(int contentId)
    {
        var content = umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext)
            ? umbracoContext.Content?.GetById(contentId)
            : null;

        return content is null ? string.Empty : await BuildSchemaAsync(content);
    }

    public string GetFaqSchema(List<IPublishedElement> faqItems)
    {
        // Create a list of Question objects
        var faqList = new List<Question>();
        foreach (var faqItem in faqItems)
        {
            var question = new Question
            {
                Name = faqItem.Value<string>("question"),
                AcceptedAnswer = new Answer
                {
                    Text = faqItem.Value<HtmlEncodedString>("answer")
                }
            };
            faqList.Add(question);
        }

        // Serialize the list to JSON
        return JsonSerializer.Serialize(faqList, _jsonSerializerOptions);
    }
}
