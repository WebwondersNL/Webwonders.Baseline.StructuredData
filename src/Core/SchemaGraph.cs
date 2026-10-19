using Schema.NET;
using Webwonders.Baseline.StructuredData.Abstractions;

namespace Webwonders.Baseline.StructuredData.Core;

public sealed class SchemaGraph : ISchemaGraph
{
    private readonly List<Thing> _nodes = new();
    // Keyed on the full id string: Uri.Equals ignores the fragment, which would merge {url}#webpage and {url}#breadcrumb.
    private readonly Dictionary<string, Thing> _byId = new(StringComparer.Ordinal);
    private readonly Dictionary<Thing, HashSet<string>> _sources = new(ReferenceEqualityComparer.Instance);

    /// <summary>The provider or processor currently contributing, recorded per node for diagnostics.</summary>
    internal string? CurrentSource { get; set; }

    public IReadOnlyList<Thing> Nodes => _nodes;

    public Thing Add(Thing node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (node.Id is null)
        {
            if (!_nodes.Contains(node, ReferenceEqualityComparer.Instance))
            {
                _nodes.Add(node);
            }

            Track(node);
            return node;
        }

        if (!_byId.TryGetValue(Key(node.Id), out var existing))
        {
            _byId[Key(node.Id)] = node;
            _nodes.Add(node);
            Track(node);
            return node;
        }

        var merged = SchemaNodeMerger.Merge(existing, node);
        if (!ReferenceEquals(merged, existing))
        {
            Replace(existing, merged);
        }

        Track(merged);
        return merged;
    }

    public T GetOrAdd<T>(Uri id, Func<T>? factory = null) where T : Thing, new()
    {
        ArgumentNullException.ThrowIfNull(id);

        if (_byId.TryGetValue(Key(id), out var existing) && existing is T typed)
        {
            Track(typed);
            return typed;
        }

        var node = factory?.Invoke() ?? new T();
        node.Id = id;

        // When a node of another type already uses this id, Add merges its values into the new node.
        return (T)Add(node);
    }

    public bool TryGet<T>(Uri id, out T node) where T : Thing
    {
        if (_byId.TryGetValue(Key(id), out var existing) && existing is T typed)
        {
            node = typed;
            return true;
        }

        node = null!;
        return false;
    }

    public bool Remove(Uri id) => _byId.TryGetValue(Key(id), out var node) && Remove(node);

    public bool Remove(Thing node)
    {
        var index = _nodes.FindIndex(n => ReferenceEquals(n, node));
        if (index < 0)
        {
            return false;
        }

        _nodes.RemoveAt(index);
        _sources.Remove(node);
        if (node.Id is not null)
        {
            _byId.Remove(Key(node.Id));
        }

        return true;
    }

    public IReadOnlyCollection<string> GetSources(Thing node)
        => _sources.TryGetValue(node, out var sources) ? sources : Array.Empty<string>();

    private static string Key(Uri id) => id.IsAbsoluteUri ? id.AbsoluteUri : id.OriginalString;

    private void Replace(Thing existing, Thing replacement)
    {
        var index = _nodes.FindIndex(n => ReferenceEquals(n, existing));
        _nodes[index] = replacement;
        _byId[Key(replacement.Id!)] = replacement;

        if (_sources.Remove(existing, out var sources))
        {
            _sources[replacement] = sources;
        }
    }

    private void Track(Thing node)
    {
        if (CurrentSource is null)
        {
            return;
        }

        if (!_sources.TryGetValue(node, out var sources))
        {
            _sources[node] = sources = new HashSet<string>();
        }

        sources.Add(CurrentSource);
    }
}
