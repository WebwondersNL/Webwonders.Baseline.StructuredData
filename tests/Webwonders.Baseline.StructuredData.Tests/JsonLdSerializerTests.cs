using System.Text.Json;
using Schema.NET;
using Webwonders.Baseline.StructuredData.Abstractions;
using Webwonders.Baseline.StructuredData.Core;

namespace Webwonders.Baseline.StructuredData.Tests;

public class JsonLdSerializerTests
{
    private static readonly Uri OrgId = new("https://example.nl/#organization");

    [Fact]
    public void Empty_graph_renders_nothing()
    {
        Assert.Null(JsonLdSerializer.Serialize(new SchemaGraph()));
        Assert.Equal(string.Empty, JsonLdSerializer.ToScriptTag(null));
    }

    [Fact]
    public void Content_cannot_close_the_script_tag()
    {
        var graph = new SchemaGraph();
        graph.Add(new Organization { Id = OrgId, Name = "Evil</script><script>alert(1)</script> & co" });

        var script = JsonLdSerializer.ToScriptTag(JsonLdSerializer.Serialize(graph));

        Assert.DoesNotContain("</script><script>", script);
        Assert.Equal(1, CountOccurrences(script, "</script>"));
        Assert.EndsWith("</script>", script);

        // The escaped value still round-trips to the original text.
        var json = JsonLdSerializer.Serialize(graph)!;
        using var document = JsonDocument.Parse(json);
        Assert.Equal("Evil</script><script>alert(1)</script> & co",
            document.RootElement.GetProperty("@graph")[0].GetProperty("name").GetString());
    }

    [Fact]
    public void Non_ascii_text_stays_readable()
    {
        var graph = new SchemaGraph();
        graph.Add(new Organization { Id = OrgId, Name = "Café Één" });

        Assert.Contains("Café Één", JsonLdSerializer.Serialize(graph));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Writes_one_context_and_serializes_nodes_with_their_runtime_type(bool writeIndented)
    {
        var graph = new SchemaGraph();
        graph.Add(new Organization { Id = OrgId, Name = "Webwonders" });
        graph.Add(new WebPage
        {
            Id = new Uri("https://example.nl/#webpage"),
            Name = "Home",
            Publisher = SchemaRef.To<Organization>(OrgId),
        });

        var json = JsonLdSerializer.Serialize(graph, writeIndented)!;
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("https://schema.org", root.GetProperty("@context").GetString());
        Assert.Equal(1, CountOccurrences(json, "@context"));

        var nodes = root.GetProperty("@graph");
        Assert.Equal(2, nodes.GetArrayLength());
        var publisher = nodes[1].GetProperty("publisher");
        Assert.Equal(OrgId.ToString(), publisher.GetProperty("@id").GetString());
    }

    private static int CountOccurrences(string text, string value)
        => (text.Length - text.Replace(value, string.Empty).Length) / value.Length;
}
