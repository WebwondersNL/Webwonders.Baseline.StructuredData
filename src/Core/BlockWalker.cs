using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Webwonders.Baseline.StructuredData.Abstractions;

namespace Webwonders.Baseline.StructuredData.Core;

/// <summary>
/// Finds the blocks in the mapped block properties of a page, including nested blocks and grid areas.
/// </summary>
internal static class BlockWalker
{
    public static IEnumerable<(SchemaBlock Block, string[] ProviderKeys)> Walk(
        IPublishedElement owner,
        IReadOnlyDictionary<string, string[]> mapping,
        int maxDepth)
        => Walk(owner, mapping, maxDepth, depth: 0);

    private static IEnumerable<(SchemaBlock, string[])> Walk(
        IPublishedElement owner,
        IReadOnlyDictionary<string, string[]> mapping,
        int maxDepth,
        int depth)
    {
        if (depth >= maxDepth)
        {
            yield break;
        }

        foreach (var (alias, keys) in mapping)
        {
            // Culture is resolved by Umbraco's variation context, invariant properties ignore it.
            var value = owner.GetProperty(alias)?.GetValue(culture: null, segment: null);

            foreach (var reference in Flatten(value))
            {
                if (reference.Content is null)
                {
                    continue;
                }

                yield return (new SchemaBlock(reference.Content, reference.Settings, alias, owner), keys);

                foreach (var nested in Walk(reference.Content, mapping, maxDepth, depth + 1))
                {
                    yield return nested;
                }
            }
        }
    }

    private static IEnumerable<IBlockReference<IPublishedElement, IPublishedElement>> Flatten(object? value)
    {
        switch (value)
        {
            case IBlockReference<IPublishedElement, IPublishedElement> single:
                foreach (var reference in WithAreas(single))
                {
                    yield return reference;
                }

                break;

            case IEnumerable<IBlockReference<IPublishedElement, IPublishedElement>> many:
                foreach (var reference in many.SelectMany(WithAreas))
                {
                    yield return reference;
                }

                break;
        }
    }

    private static IEnumerable<IBlockReference<IPublishedElement, IPublishedElement>> WithAreas(
        IBlockReference<IPublishedElement, IPublishedElement> reference)
    {
        yield return reference;

        if (reference is not BlockGridItem { Areas: { } areas })
        {
            yield break;
        }

        foreach (var item in areas.SelectMany(area => area).SelectMany(WithAreas))
        {
            yield return item;
        }
    }
}
