using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Models.PublishedContent;
using Webwonders.Baseline.StructuredData.Abstractions;

namespace Webwonders.Baseline.StructuredData.Core;

/// <summary>
/// Default <see cref="ISchemaContext"/>. Public so providers can be unit tested without booting Umbraco.
/// </summary>
public sealed class SchemaContext : ISchemaContext
{
    private readonly Func<IPublishedContent, Uri?> _urlResolver;

    // Every @id goes through AbsoluteUrl, so the same few pages are resolved many times per render.
    // Umbraco's URL provider is comparatively expensive, so the answers are remembered for this build.
    private readonly Dictionary<Guid, Uri?> _urls = new();

    /// <param name="content">The page being rendered.</param>
    /// <param name="ancestorsOrSelf">The page's ancestors including itself, root first. The first item is used as root.</param>
    /// <param name="urlResolver">Resolves the absolute URL of a content item, or null when it isn't routable.</param>
    public SchemaContext(
        IPublishedContent content,
        IReadOnlyList<IPublishedContent> ancestorsOrSelf,
        Func<IPublishedContent, Uri?> urlResolver,
        string? culture = null,
        bool isPreview = false,
        HttpContext? httpContext = null,
        ISchemaGraph? graph = null)
    {
        if (ancestorsOrSelf.Count == 0)
        {
            ancestorsOrSelf = new[] { content };
        }

        Content = content;
        AncestorsOrSelf = ancestorsOrSelf;
        Root = ancestorsOrSelf[0];
        Culture = culture;
        IsPreview = isPreview;
        HttpContext = httpContext;
        Graph = graph ?? new SchemaGraph();
        _urlResolver = urlResolver;
        Ids = new SchemaIdFactory(this);
    }

    public IPublishedContent Content { get; }

    public IPublishedContent Root { get; }

    public IReadOnlyList<IPublishedContent> AncestorsOrSelf { get; }

    public string? Culture { get; }

    public bool IsPreview { get; }

    public HttpContext? HttpContext { get; }

    public ISchemaGraph Graph { get; }

    public ISchemaIdFactory Ids { get; }

    public IDictionary<string, object?> Items { get; private init; } = new Dictionary<string, object?>();

    public Uri? AbsoluteUrl(IPublishedContent content)
    {
        if (!_urls.TryGetValue(content.Key, out var url))
        {
            _urls[content.Key] = url = _urlResolver(content);
        }

        return url;
    }

    /// <summary>A copy of this context that writes to a different graph but shares <see cref="Items"/>.</summary>
    internal SchemaContext WithGraph(ISchemaGraph graph)
        => new(Content, AncestorsOrSelf, _urlResolver, Culture, IsPreview, HttpContext, graph) { Items = Items };
}
