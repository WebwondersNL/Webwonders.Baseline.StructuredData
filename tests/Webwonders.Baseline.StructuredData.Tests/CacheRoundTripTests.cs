using Schema.NET;
using Umbraco.Cms.Core.Cache;
using Webwonders.Baseline.StructuredData.Abstractions;
using Webwonders.Baseline.StructuredData.Caching;
using Webwonders.Baseline.StructuredData.Core;

namespace Webwonders.Baseline.StructuredData.Tests;

/// <summary>
/// Cacheable providers' nodes are stored as JSON and deserialized with Schema.NET on every read.
/// These tests check that what comes out of the cache renders exactly like what went in.
/// </summary>
public class CacheRoundTripTests
{
    private static readonly Uri Site = new("https://example.nl/");

    public static TheoryData<string, Func<Thing>> Nodes() => new()
    {
        { "Organization with nested address and contact point", () => new Organization
        {
            Id = new Uri(Site, "#organization"),
            Name = "Webwonders",
            Url = Site,
            Logo = new Uri(Site, "logo.svg"),
            SameAs = new List<Uri> { new("https://www.linkedin.com/company/webwonders"), new("https://github.com/WebwondersNL") },
            Address = new PostalAddress { StreetAddress = "Klokgebouw 262", PostalCode = "5617AC", AddressLocality = "Eindhoven", AddressCountry = "NL" },
            ContactPoint = new ContactPoint { Telephone = "+31 40 8200393", ContactType = "customer service", AvailableLanguage = new List<string> { "nl", "en" } },
        } },
        { "WebSite with reference to an organization", () => new WebSite
        {
            Id = new Uri(Site, "#website"),
            Name = "Demo",
            InLanguage = "nl-NL",
            Publisher = SchemaRef.To<Organization>(new Uri(Site, "#organization")),
        } },
        { "BlogPosting with person author, dates and image", () => new BlogPosting
        {
            Id = new Uri(Site, "blog/post/#article"),
            Headline = "Structured data",
            DatePublished = new DateTime(2026, 10, 1, 9, 30, 15, DateTimeKind.Utc),
            DateModified = new DateTimeOffset(2026, 10, 2, 14, 0, 0, TimeSpan.FromHours(2)),
            Author = new Person { Name = "Jane Developer", Url = new Uri(Site, "team/jane/") },
            Image = new ImageObject { Url = new Uri(Site, "media/hero.jpg"), Width = 1200, Height = 630 },
            Keywords = "umbraco, json-ld",
            WordCount = 1234,
        } },
        { "FAQPage with questions and answers", () => new FAQPage
        {
            Id = new Uri(Site, "faq/#faq"),
            MainEntity = new List<IThing>
            {
                new Question { Name = "One?", AcceptedAnswer = new Answer { Text = "<p>First & <b>bold</b></p>" } },
                new Question { Name = "Two?", AcceptedAnswer = new Answer { Text = "Second" } },
            },
        } },
        { "BreadcrumbList with positions", () => new BreadcrumbList
        {
            Id = new Uri(Site, "a/b/#breadcrumb"),
            ItemListElement = new List<IListItem>
            {
                new ListItem { Position = 1, Name = "Home", Item = new WebPage { Id = Site } },
                new ListItem { Position = 2, Name = "A", Item = new WebPage { Id = new Uri(Site, "a/") } },
            },
        } },
        { "LocalBusiness with opening hours, geo and price range", () => new LocalBusiness
        {
            Id = new Uri(Site, "#localbusiness"),
            Name = "Shop",
            PriceRange = "€€",
            Geo = new GeoCoordinates { Latitude = 51.4477, Longitude = 5.4566 },
            OpeningHoursSpecification = new List<IOpeningHoursSpecification>
            {
                new OpeningHoursSpecification
                {
                    DayOfWeek = new List<Schema.NET.DayOfWeek?> { Schema.NET.DayOfWeek.Monday, Schema.NET.DayOfWeek.Friday },
                    Opens = new TimeSpan(9, 0, 0),
                    Closes = new TimeSpan(17, 30, 0),
                },
            },
        } },
        { "Product with offer, rating and review", () => new Product
        {
            Id = new Uri(Site, "products/x/#product"),
            Name = "Widget",
            Sku = "W-1",
            Offers = new Offer { Price = 19.95m, PriceCurrency = "EUR", Availability = ItemAvailability.InStock, Url = new Uri(Site, "products/x/") },
            AggregateRating = new AggregateRating { RatingValue = 4.6, ReviewCount = 87 },
            Review = new Review { Author = new Person { Name = "Customer" }, ReviewRating = new Rating { RatingValue = 5 } },
        } },
        { "Event with place and duration", () => new Event
        {
            Id = new Uri(Site, "events/1/#event"),
            Name = "Meetup",
            StartDate = new DateTimeOffset(2026, 11, 5, 19, 0, 0, TimeSpan.FromHours(1)),
            Duration = TimeSpan.FromMinutes(150),
            EventStatus = EventStatusType.EventScheduled,
            Location = new Place { Name = "Klokgebouw", Address = new PostalAddress { AddressLocality = "Eindhoven" } },
        } },
        { "Node without an id", () => new Thing { Name = "Anonymous", Description = "No @id" } },
    };

    [Theory]
    [MemberData(nameof(Nodes))]
    public async Task Cached_nodes_render_exactly_like_the_originals(string description, Func<Thing> create)
    {
        _ = description;
        var cache = new ProviderResultCache(new AppCaches(new ObjectCacheAppCache(), NoAppCache.Instance, new IsolatedCaches(_ => NoAppCache.Instance)));
        var context = TestContent.Context(TestContent.Tree().Home);
        var provider = new FixedProvider();

        var original = new SchemaGraph();
        original.Add(create());

        // First call stores, second call reads from the cache.
        await cache.GetOrCreateAsync(provider, context, TimeSpan.FromMinutes(1), () => Task.FromResult(original.Nodes));
        var fromCache = await cache.GetOrCreateAsync(provider, context, TimeSpan.FromMinutes(1),
            () => throw new InvalidOperationException("Should have been served from the cache"));

        var roundTripped = new SchemaGraph();
        foreach (var node in fromCache)
        {
            roundTripped.Add(node);
        }

        Assert.Equal(original.Nodes.Single().GetType(), roundTripped.Nodes.Single().GetType());
        Assert.Equal(JsonLdSerializer.Serialize(original, writeIndented: true), JsonLdSerializer.Serialize(roundTripped, writeIndented: true));
    }

    [Fact]
    public async Task Cache_keys_differ_per_culture_and_host()
    {
        var page = TestContent.Tree().Home;
        var provider = new FixedProvider();

        var keys = new[]
        {
            ProviderResultCache.CreateKey(provider, Context(page, "en-US", "localhost:44322")),
            ProviderResultCache.CreateKey(provider, Context(page, "nl-NL", "localhost:44322")),
            ProviderResultCache.CreateKey(provider, Context(page, "nl-NL", "127.0.0.1:44322")),
        };

        Assert.Equal(3, keys.Distinct().Count());
        Assert.All(keys, k => Assert.StartsWith(ProviderResultCache.KeyPrefix, k));
        await Task.CompletedTask;

        static SchemaContext Context(Umbraco.Cms.Core.Models.PublishedContent.IPublishedContent page, string culture, string host)
        {
            var http = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            http.Request.Host = new Microsoft.AspNetCore.Http.HostString(host);
            return new SchemaContext(page, new[] { page }, TestContent.Url, culture, httpContext: http);
        }
    }

    private sealed class FixedProvider : ISchemaProvider
    {
        public int Order => 0;

        public bool Cacheable => true;

        public bool AppliesTo(ISchemaContext context) => true;

        public Task ContributeAsync(ISchemaContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
