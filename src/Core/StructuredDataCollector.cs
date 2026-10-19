using Schema.NET;
using Webwonders.Baseline.StructuredData.Abstractions;

namespace Webwonders.Baseline.StructuredData.Core;

internal sealed class StructuredDataCollector : IStructuredDataCollector
{
    private readonly List<Action<ISchemaGraph>> _contributions = new();

    public void Add(Thing node)
    {
        ArgumentNullException.ThrowIfNull(node);
        _contributions.Add(graph => graph.Add(node));
    }

    public void Configure<T>(Uri id, Action<T> configure) where T : Thing, new()
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(configure);
        _contributions.Add(graph => configure(graph.GetOrAdd<T>(id)));
    }

    public void ApplyTo(ISchemaGraph graph)
    {
        foreach (var contribution in _contributions)
        {
            contribution(graph);
        }
    }
}
