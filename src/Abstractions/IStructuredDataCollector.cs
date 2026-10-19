using Schema.NET;

namespace Webwonders.Baseline.StructuredData.Abstractions;

/// <summary>
/// Request-scoped collector for controllers and views to add nodes outside of providers.
/// </summary>
/// <remarks>
/// Contributions are applied after providers and before processors, and are never cached.
/// Anything added from a view only ends up in the output if that view renders before the
/// <c>&lt;structured-data /&gt;</c> tag.
/// </remarks>
public interface IStructuredDataCollector
{
    void Add(Thing node);

    /// <summary>Changes the node with this id, creating it when no provider added it.</summary>
    void Configure<T>(Uri id, Action<T> configure) where T : Thing, new();
}
