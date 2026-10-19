using Umbraco.Cms.Core.Models.PublishedContent;

namespace Webwonders.Baseline.StructuredData.Abstractions;

/// <summary>
/// A block found in one of the block properties mapped under <c>Webwonders:StructuredData:BlockProperties</c>.
/// </summary>
/// <param name="Content">The block's content element.</param>
/// <param name="Settings">The block's settings element, if any.</param>
/// <param name="PropertyAlias">The alias of the property the block was found in.</param>
/// <param name="Owner">The page or element that holds the property.</param>
public sealed record SchemaBlock(
    IPublishedElement Content,
    IPublishedElement? Settings,
    string PropertyAlias,
    IPublishedElement Owner);

/// <summary>
/// Adds nodes to the graph for a block on the page.
/// </summary>
public interface IElementSchemaProvider
{
    /// <summary>
    /// The name used in the appsettings block mapping. Defaults to the class name without the
    /// <c>ElementSchemaProvider</c> / <c>SchemaProvider</c> suffix, e.g. <c>FaqSchemaProvider</c> → <c>Faq</c>.
    /// </summary>
    string Key { get; }

    int Order { get; }

    bool AppliesTo(ISchemaContext context, SchemaBlock block);

    Task ContributeAsync(ISchemaContext context, SchemaBlock block, CancellationToken cancellationToken);
}

/// <summary>
/// An element provider that only runs for blocks whose content is of type <typeparamref name="TElement"/>.
/// </summary>
public abstract class ElementSchemaProvider<TElement> : IElementSchemaProvider
    where TElement : class, IPublishedElement
{
    public virtual string Key => DefaultKey(GetType());

    public virtual int Order => 0;

    public bool AppliesTo(ISchemaContext context, SchemaBlock block)
        => block.Content is TElement element && AppliesTo(context, element, block);

    public Task ContributeAsync(ISchemaContext context, SchemaBlock block, CancellationToken cancellationToken)
        => ContributeAsync(context, (TElement)block.Content, block, cancellationToken);

    protected virtual bool AppliesTo(ISchemaContext context, TElement element, SchemaBlock block) => true;

    protected abstract Task ContributeAsync(ISchemaContext context, TElement element, SchemaBlock block, CancellationToken cancellationToken);

    internal static string DefaultKey(Type type)
    {
        var name = type.Name;
        foreach (var suffix in new[] { "ElementSchemaProvider", "SchemaProvider" })
        {
            if (name.EndsWith(suffix, StringComparison.Ordinal) && name.Length > suffix.Length)
            {
                return name[..^suffix.Length];
            }
        }

        return name;
    }
}
