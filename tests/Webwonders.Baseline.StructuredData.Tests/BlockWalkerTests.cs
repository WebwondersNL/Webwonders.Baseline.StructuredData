using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Webwonders.Baseline.StructuredData.Core;

namespace Webwonders.Baseline.StructuredData.Tests;

public class BlockWalkerTests
{
    private static BlockListModel List(params IPublishedElement[] elements)
        => new(elements.Select(e => new BlockListItem(e.Key, e, null, null)).ToList());

    private static Dictionary<string, string[]> Map(params string[] aliases)
        => aliases.ToDictionary(a => a, _ => new[] { "*" }, StringComparer.OrdinalIgnoreCase);

    [Fact]
    public void Only_mapped_properties_are_walked()
    {
        var faq = TestContent.Element();
        var hero = TestContent.Element();
        var page = TestContent.Element(("contentBlocks", List(faq)), ("headerBlocks", List(hero)));

        var blocks = BlockWalker.Walk(page, Map("contentBlocks"), maxDepth: 5).ToList();

        var (block, keys) = Assert.Single(blocks);
        Assert.Same(faq, block.Content);
        Assert.Equal("contentBlocks", block.PropertyAlias);
        Assert.Same(page, block.Owner);
        Assert.Equal(new[] { "*" }, keys);
    }

    [Fact]
    public void Nested_blocks_are_only_walked_when_their_alias_is_mapped()
    {
        var question = TestContent.Element();
        var mappedNested = TestContent.Element(("questions", List(question)));
        var unmappedNested = TestContent.Element(("other", List(TestContent.Element())));
        var page = TestContent.Element(("contentBlocks", List(mappedNested, unmappedNested)));

        var blocks = BlockWalker.Walk(page, Map("contentBlocks", "questions"), maxDepth: 5).Select(b => b.Block).ToList();

        Assert.Equal(new[] { mappedNested, question, unmappedNested }, blocks.Select(b => b.Content));
        Assert.Same(mappedNested, blocks[1].Owner);
        Assert.Equal("questions", blocks[1].PropertyAlias);
    }

    [Fact]
    public void Block_grid_areas_are_walked()
    {
        var inArea = TestContent.Element();
        var gridItem = new BlockGridItem(Guid.NewGuid(), TestContent.Element(), null, null)
        {
            Areas = new[] { new BlockGridArea(new List<BlockGridItem> { new(inArea.Key, inArea, null, null) }, "main", 1, 12) },
        };
        var page = TestContent.Element(("grid", new BlockGridModel(new List<BlockGridItem> { gridItem }, 12)));

        var blocks = BlockWalker.Walk(page, Map("grid"), maxDepth: 5).Select(b => b.Block).ToList();

        Assert.Equal(2, blocks.Count);
        Assert.Same(inArea, blocks[1].Content);
        Assert.Equal("grid", blocks[1].PropertyAlias);
    }

    [Fact]
    public void Walking_stops_at_the_max_depth()
    {
        var deepest = TestContent.Element();
        var middle = TestContent.Element(("blocks", List(deepest)));
        var page = TestContent.Element(("blocks", List(middle)));

        var blocks = BlockWalker.Walk(page, Map("blocks"), maxDepth: 1).ToList();

        Assert.Same(middle, Assert.Single(blocks).Block.Content);
    }

    [Fact]
    public void Missing_or_non_block_values_are_ignored()
    {
        var page = TestContent.Element(("contentBlocks", "just a string"), ("empty", null));

        Assert.Empty(BlockWalker.Walk(page, Map("contentBlocks", "empty", "doesNotExist"), maxDepth: 5));
    }
}
