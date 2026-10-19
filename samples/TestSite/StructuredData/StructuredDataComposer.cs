using Umbraco.Cms.Core.Composing;
using Webwonders.Baseline.StructuredData.DependencyInjection;

namespace Webwonders.StructuredData.TestSite.StructuredData;

/// <summary>
/// Tells the structured data package what this project wants rendered.
/// </summary>
public class StructuredDataComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.SchemaProviders()
            .Append<OrganizationSchemaProvider>()
            .Append<WebSiteSchemaProvider>()
            .Append<PageMetaSchemaProvider>()
            .Append<BlogPostSchemaProvider>();

        builder.ElementSchemaProviders()
            .Append<FaqItemSchemaProvider>();

        builder.SchemaGraphProcessors()
            .Append<LinkPageToSiteProcessor>();
    }
}
