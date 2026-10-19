using Schema.NET;
using Umbraco.Cms.Web.Common.PublishedModels;
using Webwonders.Baseline.StructuredData.Abstractions;
using Webwonders.Baseline.StructuredData.Defaults;

namespace Webwonders.StructuredData.TestSite.StructuredData;

/// <summary>
/// Runs on every page that uses the "Page Meta" composition (matched through the generated
/// <see cref="IPageMeta"/> interface) and adds the meta description to the package's default WebPage.
/// Adding a node with the same @id merges into the existing one, so only the description is added.
/// </summary>
public class PageMetaSchemaProvider : SchemaProvider<IPageMeta>
{
    protected override Task ContributeAsync(ISchemaContext context, IPageMeta content, CancellationToken cancellationToken)
    {
        context.Graph.Add(new WebPage
        {
            Id = context.Ids.ForCurrentPage(WebPageProvider.Fragment),
            Description = content.MetaDescription,
        });

        return Task.CompletedTask;
    }
}
