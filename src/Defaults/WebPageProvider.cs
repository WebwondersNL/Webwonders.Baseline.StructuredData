using Microsoft.Extensions.Options;
using Schema.NET;
using Umbraco.Cms.Core.Models.PublishedContent;
using Webwonders.Baseline.StructuredData.Abstractions;
using Webwonders.Baseline.StructuredData.Configuration;

namespace Webwonders.Baseline.StructuredData.Defaults;

/// <summary>
/// A WebPage node for every routable page with its name, URL, language and last modified date. Uses no property aliases.
/// Add to it from your own provider by adding a node with the same id: <c>ctx.Ids.ForCurrentPage(WebPageProvider.Fragment)</c>.
/// </summary>
public class WebPageProvider(IOptionsMonitor<StructuredDataOptions> options) : SchemaProvider<IPublishedContent>
{
    public const string Fragment = "webpage";

    public override int Order => -1000;

    protected override bool AppliesTo(ISchemaContext context, IPublishedContent content)
        => options.CurrentValue.Defaults.WebPage;

    protected override Task ContributeAsync(ISchemaContext context, IPublishedContent content, CancellationToken cancellationToken)
    {
        var url = context.AbsoluteUrl(content);
        if (url is null)
        {
            return Task.CompletedTask;
        }

        var page = new WebPage
        {
            Id = context.Ids.ForCurrentPage(Fragment),
            Url = url,
            Name = content.Name,
            DateModified = content.UpdateDate,
        };

        if (context.Culture is not null)
        {
            page.InLanguage = context.Culture;
        }

        var breadcrumbId = context.Ids.ForCurrentPage(BreadcrumbListProvider.Fragment);
        if (context.Graph.TryGet<BreadcrumbList>(breadcrumbId, out _))
        {
            page.Breadcrumb = SchemaRef.To<BreadcrumbList>(breadcrumbId);
        }

        context.Graph.Add(page);
        return Task.CompletedTask;
    }
}
