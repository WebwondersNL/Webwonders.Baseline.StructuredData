using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Webwonders.Baseline.StructuredData.Abstractions;

/// <summary>
/// Everything a provider needs to know about the page currently being rendered.
/// </summary>
public interface ISchemaContext
{
    /// <summary>The page the structured data is being built for.</summary>
    IPublishedContent Content { get; }

    /// <summary>The root of the site the page belongs to.</summary>
    IPublishedContent Root { get; }

    /// <summary>Ancestors of <see cref="Content"/> including itself, ordered from the root down.</summary>
    IReadOnlyList<IPublishedContent> AncestorsOrSelf { get; }

    /// <summary>The culture being rendered, or null for invariant sites.</summary>
    string? Culture { get; }

    bool IsPreview { get; }

    HttpContext? HttpContext { get; }

    /// <summary>The graph that providers add their nodes to.</summary>
    ISchemaGraph Graph { get; }

    /// <summary>Builds stable <c>@id</c> values so nodes can reference each other.</summary>
    ISchemaIdFactory Ids { get; }

    /// <summary>Free-form state that providers can share during a single build.</summary>
    IDictionary<string, object?> Items { get; }

    /// <summary>The absolute URL of a content item in the current culture, or null if it isn't routable.</summary>
    Uri? AbsoluteUrl(IPublishedContent content);
}
