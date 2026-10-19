using Schema.NET;
using Umbraco.Cms.Core.Cache;
using Webwonders.Baseline.StructuredData.Abstractions;
using Webwonders.Baseline.StructuredData.Core;

namespace Webwonders.Baseline.StructuredData.Caching;

/// <summary>
/// Caches the nodes contributed by providers that opt in with <see cref="ISchemaProvider.Cacheable"/>.
/// </summary>
/// <remarks>
/// Nodes are stored serialized and deserialized on every read, so processors that change the graph
/// later in the request can never change the cached copy.
/// </remarks>
internal sealed class ProviderResultCache(AppCaches appCaches)
{
    public const string KeyPrefix = "Webwonders.StructuredData:";

    private sealed record CachedNode(Type Type, string Json);

    public async Task<IReadOnlyList<Thing>> GetOrCreateAsync(
        ISchemaProvider provider,
        ISchemaContext context,
        TimeSpan duration,
        Func<Task<IReadOnlyList<Thing>>> factory)
    {
        var key = CreateKey(provider, context);
        var cache = appCaches.RuntimeCache;

        if (cache.Get(key) is not IReadOnlyList<CachedNode> cached)
        {
            var nodes = await factory();
            cached = nodes.Select(n => new CachedNode(n.GetType(), JsonLdSerializer.SerializeNode(n))).ToList();
            cache.Insert(key, () => cached, duration);
        }

        return cached
            .Select(n => JsonLdSerializer.DeserializeNode(n.Json, n.Type))
            .OfType<Thing>()
            .ToList();
    }

    public void Clear() => appCaches.RuntimeCache.ClearByKey(KeyPrefix);

    internal static string CreateKey(ISchemaProvider provider, ISchemaContext context)
    {
        // The host is part of the key because the same page can be served on several domains,
        // and absolute URLs end up in the output.
        var host = context.HttpContext?.Request.Host.Value ?? string.Empty;
        return $"{KeyPrefix}{provider.GetType().FullName}:{context.Content.Key}:{context.Culture}:{host}";
    }
}
