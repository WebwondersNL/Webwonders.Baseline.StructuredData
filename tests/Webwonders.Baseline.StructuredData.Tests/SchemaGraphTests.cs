using Schema.NET;
using Webwonders.Baseline.StructuredData.Core;

namespace Webwonders.Baseline.StructuredData.Tests;

public class SchemaGraphTests
{
    private static readonly Uri OrgId = new("https://example.nl/#organization");

    [Fact]
    public void Same_id_and_type_merges_with_later_values_winning_and_keeps_identity()
    {
        var graph = new SchemaGraph();
        var first = new Organization { Id = OrgId, Name = "First", Email = "info@example.nl" };

        graph.Add(first);
        var result = graph.Add(new Organization { Id = OrgId, Name = "Second", Telephone = "+31 40 000" });

        Assert.Same(first, result);
        Assert.Single(graph.Nodes);
        Assert.Equal("Second", first.Name.First());
        Assert.Equal("info@example.nl", first.Email.First());
        Assert.Equal("+31 40 000", first.Telephone.First());
    }

    [Fact]
    public void Different_type_with_same_id_lets_incoming_type_win_and_carries_compatible_values_over()
    {
        var graph = new SchemaGraph();
        graph.Add(new Organization { Id = OrgId, Name = "Webwonders", Email = "info@example.nl" });

        var result = graph.Add(new LocalBusiness { Id = OrgId, Name = "Webwonders Eindhoven", PriceRange = "€€" });

        var business = Assert.IsType<LocalBusiness>(Assert.Single(graph.Nodes));
        Assert.Same(business, result);
        Assert.Equal("Webwonders Eindhoven", business.Name.First());
        Assert.Equal("info@example.nl", business.Email.First());
        Assert.Equal("€€", business.PriceRange.First());
        Assert.True(graph.TryGet<LocalBusiness>(OrgId, out _));
    }

    [Fact]
    public void Ids_that_only_differ_by_fragment_are_different_nodes()
    {
        // Uri.Equals ignores fragments, so this guards against keying the graph on Uri.
        var graph = new SchemaGraph();

        graph.Add(new WebPage { Id = new Uri("https://example.nl/page/#webpage") });
        graph.Add(new BreadcrumbList { Id = new Uri("https://example.nl/page/#breadcrumb") });

        Assert.Equal(2, graph.Nodes.Count);
        Assert.True(graph.TryGet<WebPage>(new Uri("https://example.nl/page/#webpage"), out _));
        Assert.True(graph.TryGet<BreadcrumbList>(new Uri("https://example.nl/page/#breadcrumb"), out _));
    }

    [Fact]
    public void GetOrAdd_returns_the_same_node_so_blocks_can_aggregate()
    {
        var graph = new SchemaGraph();
        var id = new Uri("https://example.nl/faq/#faq");

        foreach (var question in new[] { "One?", "Two?", "Three?" })
        {
            var faq = graph.GetOrAdd<FAQPage>(id);
            faq.MainEntity = faq.MainEntity.Concat(new[] { new Question { Name = question } }).ToList();
        }

        var page = Assert.IsType<FAQPage>(Assert.Single(graph.Nodes));
        Assert.Equal(3, page.MainEntity.Count);
    }

    [Fact]
    public void GetOrAdd_uses_the_factory_and_assigns_the_id()
    {
        var graph = new SchemaGraph();

        var org = graph.GetOrAdd(OrgId, () => new Organization { Name = "From factory" });

        Assert.Equal(OrgId, org.Id);
        Assert.Equal("From factory", org.Name.First());
    }

    [Fact]
    public void Nodes_without_id_are_kept_separately_and_in_order()
    {
        var graph = new SchemaGraph();
        var a = new Thing { Name = "a" };
        var b = new Thing { Name = "b" };

        graph.Add(a);
        graph.Add(b);
        graph.Add(a);

        Assert.Equal(new[] { a, b }, graph.Nodes);
    }

    [Fact]
    public void Remove_by_id_removes_the_node()
    {
        var graph = new SchemaGraph();
        graph.Add(new Organization { Id = OrgId });

        Assert.True(graph.Remove(OrgId));
        Assert.Empty(graph.Nodes);
        Assert.False(graph.TryGet<Organization>(OrgId, out _));
        Assert.False(graph.Remove(OrgId));
    }

    [Fact]
    public void Sources_are_tracked_per_node_and_survive_a_type_replacement()
    {
        var graph = new SchemaGraph { CurrentSource = "Defaults" };
        graph.Add(new Organization { Id = OrgId });
        graph.CurrentSource = "ProjectProvider";
        var node = graph.Add(new LocalBusiness { Id = OrgId });

        Assert.Equal(new[] { "Defaults", "ProjectProvider" }, graph.GetSources(node));
    }
}
