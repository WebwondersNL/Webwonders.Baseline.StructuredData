using Umbraco.Cms.Core.Models.PublishedContent;
using Webwonders.Baseline.StructuredData.Abstractions;

namespace Webwonders.Baseline.StructuredData.Core;

internal sealed class SchemaIdFactory(ISchemaContext context) : ISchemaIdFactory
{
    public Uri For(IPublishedContent content, string fragment)
    {
        var url = context.AbsoluteUrl(content)
            ?? throw new InvalidOperationException($"Cannot create a schema @id for content {content.Key} because it has no routable URL.");

        return WithFragment(url, fragment);
    }

    public Uri ForSite(string fragment) => For(context.Root, fragment);

    public Uri ForCurrentPage(string fragment) => For(context.Content, fragment);

    private static Uri WithFragment(Uri url, string fragment)
        => new UriBuilder(url) { Fragment = fragment.TrimStart('#') }.Uri;
}
