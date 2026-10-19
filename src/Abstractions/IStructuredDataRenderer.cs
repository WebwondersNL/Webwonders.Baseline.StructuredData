using Umbraco.Cms.Core.Models.PublishedContent;

namespace Webwonders.Baseline.StructuredData.Abstractions;

/// <summary>
/// Builds and renders the structured data for a page.
/// </summary>
public interface IStructuredDataRenderer
{
    /// <summary>
    /// Runs all providers, block providers, collector contributions and processors for the page.
    /// Uses the current request's page when <paramref name="content"/> is null.
    /// Returns null when there is no page or structured data is disabled.
    /// </summary>
    Task<ISchemaGraph?> BuildGraphAsync(IPublishedContent? content = null, CancellationToken cancellationToken = default);

    /// <summary>The JSON-LD document for the page, or null when the graph is empty.</summary>
    Task<string?> RenderJsonAsync(IPublishedContent? content = null, CancellationToken cancellationToken = default);

    /// <summary>The complete <c>&lt;script type="application/ld+json"&gt;</c> tag, or an empty string.</summary>
    Task<string> RenderScriptAsync(IPublishedContent? content = null, CancellationToken cancellationToken = default);
}
