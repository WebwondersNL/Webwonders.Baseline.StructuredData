using Umbraco.Cms.Core.Models.PublishedContent;

namespace Webwonders.Baseline.StructuredData.Abstractions;

/// <summary>
/// Builds <c>@id</c> values in the form <c>{absolute url}#{fragment}</c>.
/// </summary>
public interface ISchemaIdFactory
{
    /// <summary>An id scoped to a page, e.g. <c>https://site.nl/blog/post/#webpage</c>.</summary>
    Uri For(IPublishedContent content, string fragment);

    /// <summary>An id scoped to the site root, e.g. <c>https://site.nl/#organization</c>.</summary>
    Uri ForSite(string fragment);

    /// <summary>An id scoped to the page currently being rendered.</summary>
    Uri ForCurrentPage(string fragment);
}
