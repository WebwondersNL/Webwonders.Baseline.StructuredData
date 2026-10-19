using Microsoft.Extensions.Options;
using Schema.NET;
using Umbraco.Cms.Core.Models.PublishedContent;
using Webwonders.Baseline.StructuredData.Abstractions;
using Webwonders.Baseline.StructuredData.Configuration;

namespace Webwonders.Baseline.StructuredData.Defaults;

/// <summary>
/// A BreadcrumbList for every page, built from its ancestors' names and URLs. Uses no property aliases.
/// Turn it off with <c>Defaults:BreadcrumbList = false</c> or <c>builder.SchemaProviders().Remove&lt;BreadcrumbListProvider&gt;()</c>.
/// </summary>
public class BreadcrumbListProvider(IOptionsMonitor<StructuredDataOptions> options) : SchemaProvider<IPublishedContent>
{
    public const string Fragment = "breadcrumb";

    public override int Order => -1100;

    protected override bool AppliesTo(ISchemaContext context, IPublishedContent content)
        => options.CurrentValue.Defaults.BreadcrumbList;

    protected override Task ContributeAsync(ISchemaContext context, IPublishedContent content, CancellationToken cancellationToken)
    {
        var items = new List<IListItem>();

        foreach (var page in context.AncestorsOrSelf)
        {
            var url = context.AbsoluteUrl(page);
            if (url is null)
            {
                continue;
            }

            items.Add(new ListItem
            {
                Position = items.Count + 1,
                Name = page.Name,
                Item = new WebPage { Id = url },
            });
        }

        if (items.Count > 0 && context.AbsoluteUrl(content) is not null)
        {
            context.Graph.Add(new BreadcrumbList
            {
                Id = context.Ids.ForCurrentPage(Fragment),
                ItemListElement = items,
            });
        }

        return Task.CompletedTask;
    }
}
