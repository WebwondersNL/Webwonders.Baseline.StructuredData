using Umbraco.Cms.Core.Models.PublishedContent;

namespace Webwonders.Baseline.StructuredData.Abstractions;

/// <summary>
/// Adds nodes to the graph for the page being rendered.
/// </summary>
public interface ISchemaProvider
{
    /// <summary>Providers run in ascending order; on an <c>@id</c> conflict the later one wins.</summary>
    int Order { get; }

    /// <summary>
    /// When true, the nodes this provider adds are cached per page, culture and host until content is published.
    /// A cacheable provider runs against an empty graph, so it must not depend on nodes from other providers.
    /// </summary>
    bool Cacheable { get; }

    bool AppliesTo(ISchemaContext context);

    Task ContributeAsync(ISchemaContext context, CancellationToken cancellationToken);
}

/// <summary>
/// A provider that only runs for pages of type <typeparamref name="TContent"/>. This can be a ModelsBuilder model
/// or a composition interface, so a provider for <c>ISeoComposition</c> runs on every page that uses it.
/// Use <see cref="IPublishedContent"/> to run on every page.
/// </summary>
public abstract class SchemaProvider<TContent> : ISchemaProvider
    where TContent : class
{
    public virtual int Order => 0;

    public virtual bool Cacheable => false;

    public bool AppliesTo(ISchemaContext context)
        => context.Content is TContent content && AppliesTo(context, content);

    public Task ContributeAsync(ISchemaContext context, CancellationToken cancellationToken)
        => ContributeAsync(context, (TContent)context.Content, cancellationToken);

    /// <summary>Further narrows down when the provider runs, after the type check has passed.</summary>
    protected virtual bool AppliesTo(ISchemaContext context, TContent content) => true;

    protected abstract Task ContributeAsync(ISchemaContext context, TContent content, CancellationToken cancellationToken);
}
