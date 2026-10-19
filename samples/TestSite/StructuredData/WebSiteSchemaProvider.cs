using Schema.NET;
using Umbraco.Cms.Core.Models.PublishedContent;
using Webwonders.Baseline.StructuredData.Abstractions;

namespace Webwonders.StructuredData.TestSite.StructuredData;

/// <summary>
/// The package doesn't ship a WebSite default, so the project adds one.
/// </summary>
public class WebSiteSchemaProvider : SchemaProvider<IPublishedContent>
{
    public const string Fragment = "website";

    public override bool Cacheable => true;

    protected override Task ContributeAsync(ISchemaContext context, IPublishedContent content, CancellationToken cancellationToken)
    {
        context.Graph.Add(new WebSite
        {
            Id = context.Ids.ForSite(Fragment),
            Name = context.Root.Name,
            Url = context.AbsoluteUrl(context.Root),
            InLanguage = context.Culture,
            Publisher = SchemaRef.To<Organization>(context.Ids.ForSite(OrganizationSchemaProvider.Fragment)),
        });

        return Task.CompletedTask;
    }
}
