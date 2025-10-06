
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.UmbracoContext;
using Umbraco.Extensions;
using Webwonders.Baseline.StructuredData.Converters;
using Webwonders.Baseline.StructuredData.Interfaces;
using Webwonders.Baseline.StructuredData.Models.Base;
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
}