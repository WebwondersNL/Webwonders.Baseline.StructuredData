using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Webwonders.Baseline.StructuredData.Caching;

/// <summary>
/// Clears all cached provider results when content changes. Cacheable providers may read other content
/// (e.g. a settings node), so everything is cleared rather than just the changed page.
/// The cache refresher notification is raised on every server, so this also works when load balancing.
/// </summary>
internal sealed class ClearStructuredDataCacheHandler(ProviderResultCache cache)
    : INotificationHandler<ContentCacheRefresherNotification>
{
    public void Handle(ContentCacheRefresherNotification notification) => cache.Clear();
}
