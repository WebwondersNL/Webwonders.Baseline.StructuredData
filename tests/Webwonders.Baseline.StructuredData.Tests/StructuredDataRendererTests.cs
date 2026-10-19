using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Schema.NET;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Webwonders.Baseline.StructuredData.Abstractions;
using Webwonders.Baseline.StructuredData.Caching;
using Webwonders.Baseline.StructuredData.Configuration;
using Webwonders.Baseline.StructuredData.Core;
using Webwonders.Baseline.StructuredData.DependencyInjection;
using Webwonders.Baseline.StructuredData.Validation;

namespace Webwonders.Baseline.StructuredData.Tests;

public class StructuredDataRendererTests
{
    private readonly StructuredDataOptions _options = new() { Defaults = { BreadcrumbList = false, WebPage = false } };
    private readonly ProviderResultCache _cache = new(new AppCaches(new ObjectCacheAppCache(), NoAppCache.Instance, new IsolatedCaches(_ => NoAppCache.Instance)));
    private readonly StructuredDataCollector _collector = new();
    private bool _preview;

    private StructuredDataRenderer CreateRenderer(
        IEnumerable<ISchemaProvider>? providers = null,
        IEnumerable<IElementSchemaProvider>? elementProviders = null,
        IEnumerable<ISchemaGraphProcessor>? processors = null,
        params IPublishedContent[] tree)
    {
        var umbracoContext = Mock.Of<IUmbracoContext>(c => c.InPreviewMode == _preview);
        var umbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        umbracoContextAccessor.Setup(a => a.TryGetUmbracoContext(out umbracoContext)).Returns(true);

        var urlProvider = new Mock<IPublishedUrlProvider>();
        urlProvider
            .Setup(p => p.GetUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns<IPublishedContent, UrlMode, string?, Uri?>((content, _, _, _) => TestContent.Url(content)?.ToString() ?? "#");

        // Mirrors Umbraco: ancestors exclude the node itself, AncestorsOrSelf adds it.
        var navigation = new Mock<IDocumentNavigationQueryService>();
        navigation
            .Setup(n => n.TryGetAncestorsKeys(It.IsAny<Guid>(), out It.Ref<IEnumerable<Guid>>.IsAny))
            .Returns(new TryGetKeys((Guid key, out IEnumerable<Guid> keys) =>
            {
                keys = tree.Where(c => c.Key != key).Select(c => c.Key).ToList();
                return true;
            }));
        navigation
            .Setup(n => n.TryGetAncestorsOrSelfKeys(It.IsAny<Guid>(), out It.Ref<IEnumerable<Guid>>.IsAny))
            .Returns(new TryGetKeys((Guid _, out IEnumerable<Guid> keys) =>
            {
                keys = tree.Select(c => c.Key).ToList();
                return true;
            }));

        var filtering = new Mock<IPublishedContentStatusFilteringService>();
        filtering
            .Setup(f => f.FilterAvailable(It.IsAny<IEnumerable<Guid>>(), It.IsAny<string?>()))
            .Returns<IEnumerable<Guid>, string?>((keys, _) => tree.Where(c => keys.Contains(c.Key)).ToList());

        var contextFactory = new SchemaContextFactory(
            umbracoContextAccessor.Object,
            Mock.Of<IVariationContextAccessor>(),
            urlProvider.Object,
            navigation.Object,
            filtering.Object,
            Mock.Of<IHttpContextAccessor>(a => a.HttpContext == new DefaultHttpContext()));

        return new StructuredDataRenderer(
            contextFactory,
            new SchemaProviderCollection(() => providers ?? Array.Empty<ISchemaProvider>()),
            new ElementSchemaProviderCollection(() => elementProviders ?? Array.Empty<IElementSchemaProvider>()),
            new SchemaGraphProcessorCollection(() => processors ?? Array.Empty<ISchemaGraphProcessor>()),
            _collector,
            _cache,
            new SchemaGraphValidator(Array.Empty<IRichResultRule>(), NullLogger<SchemaGraphValidator>.Instance),
            Mock.Of<IOptionsSnapshot<StructuredDataOptions>>(o => o.Value == _options),
            Mock.Of<IHostEnvironment>(e => e.EnvironmentName == Environments.Production),
            NullLogger<StructuredDataRenderer>.Instance);
    }

    [Fact]
    public async Task Providers_run_in_order_and_the_later_one_wins_on_conflicts()
    {
        var (home, _, _) = TestContent.Tree();
        var renderer = CreateRenderer(
            providers: new ISchemaProvider[]
            {
                new DelegateProvider(order: 10, ctx => ctx.Graph.Add(new Organization { Id = ctx.Ids.ForSite("org"), Name = "Late" })),
                new DelegateProvider(order: 0, ctx => ctx.Graph.Add(new Organization { Id = ctx.Ids.ForSite("org"), Name = "Early", Email = "a@b.nl" })),
            },
            tree: home);

        var graph = await renderer.BuildGraphAsync(home);

        var org = Assert.IsType<Organization>(Assert.Single(graph!.Nodes));
        Assert.Equal("Late", org.Name.First());
        Assert.Equal("a@b.nl", org.Email.First());
        Assert.Equal(new Uri("https://example.nl/#org"), org.Id);
    }

    [Fact]
    public async Task Ancestors_are_resolved_root_first_through_umbraco_navigation()
    {
        var (home, blog, post) = TestContent.Tree();
        IReadOnlyList<IPublishedContent>? seen = null;
        var renderer = CreateRenderer(
            providers: new[] { new DelegateProvider(0, ctx => seen = ctx.AncestorsOrSelf) },
            tree: new[] { post, home, blog });

        await renderer.BuildGraphAsync(post);

        Assert.Equal(new[] { home, blog, post }, seen);
    }

    [Fact]
    public async Task Element_providers_only_run_for_their_mapped_keys_and_can_aggregate()
    {
        var page = PageWithBlocks(("faqBlocks", new[] { Question("One?"), Question("Two?") }));
        _options.BlockProperties["faqBlocks"] = new[] { "Faq" };

        var renderer = CreateRenderer(
            elementProviders: new IElementSchemaProvider[] { new FaqSchemaProvider(), new ReviewSchemaProvider() },
            tree: page);

        var graph = await renderer.BuildGraphAsync(page);

        var faq = Assert.IsType<FAQPage>(Assert.Single(graph!.Nodes));
        Assert.Equal(new[] { "One?", "Two?" }, faq.MainEntity.OfType<Question>().Select(q => q.Name.First()));
    }

    [Fact]
    public async Task Collector_contributions_and_processors_are_applied_after_providers()
    {
        var home = TestContent.Tree().Home;
        var orgId = new Uri("https://example.nl/#org");
        _collector.Configure<Organization>(orgId, o => o.Telephone = "+31 40 000");
        var renderer = CreateRenderer(
            providers: new[] { new DelegateProvider(0, ctx => ctx.Graph.Add(new Organization { Id = orgId, Name = "Webwonders" })) },
            processors: new[] { new DelegateProcessor(ctx => ctx.Graph.Nodes.OfType<Organization>().Single().Name = "Processed") },
            tree: home);

        var graph = await renderer.BuildGraphAsync(home);

        var org = Assert.IsType<Organization>(Assert.Single(graph!.Nodes));
        Assert.Equal("Processed", org.Name.First());
        Assert.Equal("+31 40 000", org.Telephone.First());
    }

    [Fact]
    public async Task A_failing_provider_is_skipped_without_breaking_the_rest()
    {
        var home = TestContent.Tree().Home;
        var renderer = CreateRenderer(
            providers: new ISchemaProvider[]
            {
                new DelegateProvider(0, _ => throw new InvalidOperationException("boom")),
                new DelegateProvider(1, ctx => ctx.Graph.Add(new Organization { Id = ctx.Ids.ForSite("org") })),
            },
            tree: home);

        var json = await renderer.RenderJsonAsync(home);

        Assert.Contains("Organization", json);
    }

    [Fact]
    public async Task Cacheable_providers_run_once_and_return_independent_copies()
    {
        var home = TestContent.Tree().Home;
        var calls = 0;
        var provider = new DelegateProvider(0, ctx =>
        {
            calls++;
            ctx.Graph.Add(new Organization { Id = ctx.Ids.ForSite("org"), Name = "Cached" });
        }, cacheable: true);
        var mutate = new DelegateProcessor(ctx => ctx.Graph.Nodes.OfType<Organization>().Single().Name = "Mutated");

        var first = await CreateRenderer(new[] { provider }, processors: new[] { mutate }, tree: home).BuildGraphAsync(home);
        var second = await CreateRenderer(new[] { provider }, tree: home).BuildGraphAsync(home);

        Assert.Equal(1, calls);
        Assert.Equal("Mutated", first!.Nodes.OfType<Organization>().Single().Name.First());
        Assert.Equal("Cached", second!.Nodes.OfType<Organization>().Single().Name.First());

        _cache.Clear();
        await CreateRenderer(new[] { provider }, tree: home).BuildGraphAsync(home);
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task Cache_is_bypassed_in_preview()
    {
        var home = TestContent.Tree().Home;
        var calls = 0;
        var provider = new DelegateProvider(0, _ => calls++, cacheable: true);
        _preview = true;

        await CreateRenderer(new[] { provider }, tree: home).BuildGraphAsync(home);
        await CreateRenderer(new[] { provider }, tree: home).BuildGraphAsync(home);

        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task Disabled_or_missing_content_renders_nothing()
    {
        var home = TestContent.Tree().Home;
        var renderer = CreateRenderer(new[] { new DelegateProvider(0, ctx => ctx.Graph.Add(new Thing { Name = "x" })) }, tree: home);

        Assert.Equal(string.Empty, await renderer.RenderScriptAsync());

        _options.Enabled = false;
        Assert.Null(await renderer.BuildGraphAsync(home));
    }

    private delegate bool TryGetKeys(Guid key, out IEnumerable<Guid> keys);

    private static IPublishedElement Question(string text) => new QuestionElement(text);

    private static IPublishedContent PageWithBlocks(params (string Alias, IPublishedElement[] Blocks)[] properties)
    {
        var page = new Mock<IPublishedContent>();
        page.SetupGet(c => c.Key).Returns(Guid.NewGuid());
        page.SetupGet(c => c.Name).Returns("FAQ");
        page.SetupGet(c => c.Level).Returns(1);
        page.SetupGet(c => c.UrlSegment).Returns("/faq/");
        page.Setup(c => c.GetProperty(It.IsAny<string>())).Returns<string>(alias =>
        {
            var match = properties.FirstOrDefault(p => p.Alias == alias);
            if (match.Alias is null)
            {
                return null;
            }

            var value = new BlockListModel(match.Blocks.Select(b => new BlockListItem(b.Key, b, null, null)).ToList());
            return Mock.Of<IPublishedProperty>(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>()) == value);
        });
        return page.Object;
    }

    private sealed class DelegateProvider(int order, Action<ISchemaContext> contribute, bool cacheable = false) : ISchemaProvider
    {
        public int Order => order;

        public bool Cacheable => cacheable;

        public bool AppliesTo(ISchemaContext context) => true;

        public Task ContributeAsync(ISchemaContext context, CancellationToken cancellationToken)
        {
            contribute(context);
            return Task.CompletedTask;
        }
    }

    private sealed class DelegateProcessor(Action<ISchemaContext> process) : ISchemaGraphProcessor
    {
        public int Order => 0;

        public Task ProcessAsync(ISchemaContext context, CancellationToken cancellationToken)
        {
            process(context);
            return Task.CompletedTask;
        }
    }

    /// <summary>Stands in for a ModelsBuilder element type.</summary>
    private sealed class QuestionElement(string text) : IPublishedElement
    {
        public string Text => text;

        public IPublishedContentType ContentType => null!;

        public Guid Key { get; } = Guid.NewGuid();

        public IEnumerable<IPublishedProperty> Properties => Array.Empty<IPublishedProperty>();

        public IPublishedProperty? GetProperty(string alias) => null;
    }

    private sealed class FaqSchemaProvider : ElementSchemaProvider<QuestionElement>
    {
        protected override Task ContributeAsync(ISchemaContext context, QuestionElement element, SchemaBlock block, CancellationToken cancellationToken)
        {
            var faq = context.Graph.GetOrAdd<FAQPage>(context.Ids.ForCurrentPage("faq"));
            faq.MainEntity = faq.MainEntity.Append(new Question { Name = element.Text }).ToList();
            return Task.CompletedTask;
        }
    }

    private sealed class ReviewSchemaProvider : ElementSchemaProvider<QuestionElement>
    {
        protected override Task ContributeAsync(ISchemaContext context, QuestionElement element, SchemaBlock block, CancellationToken cancellationToken)
        {
            context.Graph.Add(new Review { Name = "Should not run, not mapped" });
            return Task.CompletedTask;
        }
    }
}
