
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.UmbracoContext;
using Umbraco.Extensions;
using Webwonders.Baseline.StructuredData.Converters;
using Webwonders.Baseline.StructuredData.Interfaces;
using Webwonders.Baseline.StructuredData.Models.Base;
using Webwonders.Baseline.StructuredData.Models.Old;
using Webwonders.Baseline.StructuredData.Models.Schemas;

namespace Webwonders.Baseline.StructuredData.Services;

public class StructuredDataService : IStructuredDataService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new SchemaEntityConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    public async Task<string> BuildSchemaAsync(IPublishedContent content)
    {
        var root = content.Root();
        var currentDocumentAlias = content.ContentType.Alias;

        var graphSchema = new GraphSchema()
        {
            Graph = new List<SchemaEntity>()
        };
        
        return JsonSerializer.Serialize(graphSchema, _jsonSerializerOptions);
    }

    public async Task<string> BuildSchemaAsync(int contentId)
    {
        throw new NotImplementedException();
    }
    
    public string GetFaqSchema(List<BlockListItem> faqItems)
    {
        // Create a list of Question objects
        var faqList = new List<Question>();
        foreach (var faqItem in faqItems)
        {
            var question = new Question
            {
                Name = faqItem.Content.Value<string>("question"),
                AcceptedAnswer = new Answer
                {
                    Text = faqItem.Content.Value<HtmlEncodedString>("answer")
                }
            };
            faqList.Add(question);
        }

        // Serialize the list to JSON
        return JsonSerializer.Serialize(faqList, _jsonSerializerOptions);
    }
}