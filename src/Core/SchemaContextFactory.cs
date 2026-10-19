using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Webwonders.Baseline.StructuredData.Core;

internal sealed class SchemaContextFactory(
    IUmbracoContextAccessor umbracoContextAccessor,
    IVariationContextAccessor variationContextAccessor,
    IPublishedUrlProvider publishedUrlProvider,
    IDocumentNavigationQueryService navigationQueryService,
    IPublishedContentStatusFilteringService publishedStatusFilteringService,
    IHttpContextAccessor httpContextAccessor)
{
    public IPublishedContent? GetCurrentContent()
        => umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext)
            ? umbracoContext.PublishedRequest?.PublishedContent
            : null;

    public SchemaContext Create(IPublishedContent content)
    {
        var culture = variationContextAccessor.VariationContext?.Culture;
        var isPreview = umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext) && umbracoContext.InPreviewMode;

        var ancestorsOrSelf = content
            .AncestorsOrSelf(navigationQueryService, publishedStatusFilteringService)
            .OrderBy(x => x.Level)
            .ToList();

        return new SchemaContext(
            content,
            ancestorsOrSelf,
            item => ResolveUrl(item, culture),
            string.IsNullOrEmpty(culture) ? null : culture,
            isPreview,
            httpContextAccessor.HttpContext);
    }

    private Uri? ResolveUrl(IPublishedContent content, string? culture)
    {
        var url = publishedUrlProvider.GetUrl(content, UrlMode.Absolute, culture);
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri : null;
    }
}
