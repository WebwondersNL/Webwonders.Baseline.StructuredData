using Schema.NET;

namespace Webwonders.Baseline.StructuredData.Abstractions;

/// <summary>
/// Creates reference nodes that only carry an <c>@id</c>, so a node can point at another node in the graph
/// without repeating it, e.g. <c>Publisher = SchemaRef.To&lt;Organization&gt;(ctx.Ids.ForSite("organization"))</c>.
/// </summary>
public static class SchemaRef
{
    public static T To<T>(Uri id) where T : Thing, new() => new() { Id = id };
}
