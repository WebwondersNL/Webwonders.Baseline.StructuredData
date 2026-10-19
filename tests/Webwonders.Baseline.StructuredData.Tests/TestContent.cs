using Moq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Webwonders.Baseline.StructuredData.Core;

namespace Webwonders.Baseline.StructuredData.Tests;

internal static class TestContent
{
    public const string Host = "https://example.nl";

    public static IPublishedContent Page(string name, int level, string path)
    {
        var mock = new Mock<IPublishedContent>();
        mock.SetupGet(c => c.Key).Returns(Guid.NewGuid());
        mock.SetupGet(c => c.Id).Returns(Random.Shared.Next(1000, 9999));
        mock.SetupGet(c => c.Name).Returns(name);
        mock.SetupGet(c => c.Level).Returns(level);
        mock.SetupGet(c => c.UrlSegment).Returns(path);
        mock.SetupGet(c => c.UpdateDate).Returns(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc));
        return mock.Object;
    }

    /// <summary>A home → blog → post tree, where every page's URL is derived from its UrlSegment.</summary>
    public static (IPublishedContent Home, IPublishedContent Blog, IPublishedContent Post) Tree()
        => (Page("Home", 1, "/"), Page("Blog", 2, "/blog/"), Page("My post", 3, "/blog/my-post/"));

    public static Uri? Url(IPublishedContent content)
        => content.UrlSegment is null ? null : new Uri(Host + content.UrlSegment);

    public static SchemaContext Context(IPublishedContent content, IReadOnlyList<IPublishedContent>? ancestorsOrSelf = null, string? culture = null)
        => new(content, ancestorsOrSelf ?? new[] { content }, Url, culture);

    public static IPublishedElement Element(params (string Alias, object? Value)[] properties)
    {
        var mock = new Mock<IPublishedElement>();
        mock.SetupGet(e => e.Key).Returns(Guid.NewGuid());
        mock.Setup(e => e.GetProperty(It.IsAny<string>())).Returns<string>(alias =>
        {
            var match = properties.FirstOrDefault(p => p.Alias == alias);
            if (match.Alias is null)
            {
                return null;
            }

            var property = new Mock<IPublishedProperty>();
            property.Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(match.Value);
            return property.Object;
        });
        return mock.Object;
    }
}
