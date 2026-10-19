using Microsoft.Extensions.Options;
using Moq;
using Schema.NET;
using Webwonders.Baseline.StructuredData.Abstractions;
using Webwonders.Baseline.StructuredData.Configuration;
using Webwonders.Baseline.StructuredData.Defaults;

namespace Webwonders.Baseline.StructuredData.Tests;

public class DefaultProviderTests
{
    private static IOptionsMonitor<StructuredDataOptions> Options(Action<StructuredDataOptions>? configure = null)
    {
        var options = new StructuredDataOptions();
        configure?.Invoke(options);
        return Mock.Of<IOptionsMonitor<StructuredDataOptions>>(m => m.CurrentValue == options);
    }

    private static async Task RunAsync(ISchemaContext context, params ISchemaProvider[] providers)
    {
        foreach (var provider in providers.OrderBy(p => p.Order).Where(p => p.AppliesTo(context)))
        {
            await provider.ContributeAsync(context, CancellationToken.None);
        }
    }

    [Fact]
    public async Task Breadcrumb_lists_ancestors_from_the_root_with_absolute_urls()
    {
        var (home, blog, post) = TestContent.Tree();
        var context = TestContent.Context(post, new[] { home, blog, post });

        await RunAsync(context, new BreadcrumbListProvider(Options()));

        var breadcrumb = Assert.IsType<BreadcrumbList>(Assert.Single(context.Graph.Nodes));
        Assert.Equal(new Uri("https://example.nl/blog/my-post/#breadcrumb"), breadcrumb.Id);

        var items = breadcrumb.ItemListElement.Cast<ListItem>().ToList();
        Assert.Equal(new[] { "Home", "Blog", "My post" }, items.Select(i => i.Name.First()));
        Assert.Equal(new[] { 1, 2, 3 }, items.Select(i => (int)i.Position.First()!));
        Assert.Equal(new Uri("https://example.nl/blog/"), Assert.IsType<WebPage>(items[1].Item.First()).Id);
    }

    [Fact]
    public async Task WebPage_links_to_the_breadcrumb_and_sets_language()
    {
        var (home, _, post) = TestContent.Tree();
        var context = TestContent.Context(post, new[] { home, post }, culture: "nl-NL");

        await RunAsync(context, new WebPageProvider(Options()), new BreadcrumbListProvider(Options()));

        Assert.Equal(2, context.Graph.Nodes.Count);
        Assert.Single(context.Graph.Nodes.OfType<BreadcrumbList>());
        var page = context.Graph.Nodes.OfType<WebPage>().Single();
        Assert.Equal(new Uri("https://example.nl/blog/my-post/#webpage"), page.Id);
        Assert.Equal("My post", page.Name.First());
        Assert.Equal("nl-NL", page.InLanguage.First());
        Assert.Equal(new Uri("https://example.nl/blog/my-post/#breadcrumb"), page.Breadcrumb.OfType<BreadcrumbList>().Single().Id);
    }

    [Fact]
    public async Task Defaults_can_be_switched_off_through_options()
    {
        var post = TestContent.Tree().Post;
        var context = TestContent.Context(post);
        var options = Options(o =>
        {
            o.Defaults.BreadcrumbList = false;
            o.Defaults.WebPage = false;
        });

        await RunAsync(context, new WebPageProvider(options), new BreadcrumbListProvider(options));

        Assert.Empty(context.Graph.Nodes);
    }

    [Fact]
    public async Task Project_provider_can_extend_the_default_webpage_by_id()
    {
        var post = TestContent.Tree().Post;
        var context = TestContent.Context(post);

        await RunAsync(context, new WebPageProvider(Options()), new ExtendWebPageProvider());

        var page = Assert.IsType<WebPage>(Assert.Single(context.Graph.Nodes));
        Assert.Equal("My post", page.Name.First());
        Assert.Equal("Extended", page.Description.First());
    }

    [Fact]
    public async Task Pages_without_a_url_get_no_defaults()
    {
        var page = TestContent.Page("Folder", 1, null!);
        var context = TestContent.Context(page);

        await RunAsync(context, new WebPageProvider(Options()), new BreadcrumbListProvider(Options()));

        Assert.Empty(context.Graph.Nodes);
    }

    [Fact]
    public void Absolute_urls_are_resolved_once_per_page_per_build()
    {
        var (home, _, post) = TestContent.Tree();
        var calls = 0;
        var context = new Core.SchemaContext(post, new[] { home, post }, c => { calls++; return TestContent.Url(c); });

        for (var i = 0; i < 5; i++)
        {
            context.Ids.ForCurrentPage("webpage");
            context.Ids.ForSite("organization");
        }

        Assert.Equal(2, calls);
    }

    private sealed class ExtendWebPageProvider : SchemaProvider<Umbraco.Cms.Core.Models.PublishedContent.IPublishedContent>
    {
        protected override Task ContributeAsync(ISchemaContext context, Umbraco.Cms.Core.Models.PublishedContent.IPublishedContent content, CancellationToken cancellationToken)
        {
            context.Graph.Add(new WebPage { Id = context.Ids.ForCurrentPage(WebPageProvider.Fragment), Description = "Extended" });
            return Task.CompletedTask;
        }
    }
}
