using Schema.NET;
using Webwonders.Baseline.StructuredData.Abstractions;
using Webwonders.Baseline.StructuredData.Defaults;

namespace Webwonders.StructuredData.TestSite.StructuredData;

/// <summary>
/// Runs after all providers: connects the package's default WebPage to this project's WebSite and Organization.
/// </summary>
public class LinkPageToSiteProcessor : ISchemaGraphProcessor
{
    public int Order => 0;

    public Task ProcessAsync(ISchemaContext context, CancellationToken cancellationToken)
    {
        if (!context.Graph.TryGet<WebPage>(context.Ids.ForCurrentPage(WebPageProvider.Fragment), out var page))
        {
            return Task.CompletedTask;
        }

        var webSiteId = context.Ids.ForSite(WebSiteSchemaProvider.Fragment);
        if (context.Graph.TryGet<WebSite>(webSiteId, out _))
        {
            page.IsPartOf = SchemaRef.To<WebSite>(webSiteId);
        }

        var organizationId = context.Ids.ForSite(OrganizationSchemaProvider.Fragment);
        if (context.Graph.TryGet<Organization>(organizationId, out _))
        {
            page.Publisher = SchemaRef.To<Organization>(organizationId);
        }

        return Task.CompletedTask;
    }
}
