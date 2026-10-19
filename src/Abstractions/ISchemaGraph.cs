using Schema.NET;

namespace Webwonders.Baseline.StructuredData.Abstractions;

/// <summary>
/// The collection of nodes that ends up in the page's <c>@graph</c>.
/// </summary>
/// <remarks>
/// Nodes with an <c>@id</c> are unique. Adding a node with an id that already exists merges it into the
/// existing node: properties set on the incoming node win, properties it leaves empty are kept.
/// When the types differ the incoming type wins and compatible values from the existing node are carried over.
/// </remarks>
public interface ISchemaGraph
{
    IReadOnlyList<Thing> Nodes { get; }

    /// <summary>Adds a node, merging it with an existing node that has the same <c>@id</c>.</summary>
    /// <returns>The node as it now lives in the graph.</returns>
    Thing Add(Thing node);

    /// <summary>Returns the node with this id, or adds a new one. Use this to aggregate (e.g. FAQ blocks into one FAQPage).</summary>
    T GetOrAdd<T>(Uri id, Func<T>? factory = null) where T : Thing, new();

    bool TryGet<T>(Uri id, out T node) where T : Thing;

    bool Remove(Uri id);

    bool Remove(Thing node);

    /// <summary>The providers and processors that contributed to a node, for diagnostics.</summary>
    IReadOnlyCollection<string> GetSources(Thing node);
}
