using Schema.NET;
using Webwonders.Baseline.StructuredData.Core;

namespace Webwonders.Baseline.StructuredData.Validation;

/// <summary>
/// Checks a node for properties Google needs before it's eligible for a rich result.
/// Only used in Development. Register your own with <c>services.AddSingleton&lt;IRichResultRule, ...&gt;()</c>.
/// </summary>
public interface IRichResultRule
{
    bool AppliesTo(Thing node);

    /// <summary>Returns a message for every problem found.</summary>
    IEnumerable<string> Validate(Thing node);
}

/// <summary>
/// A rule that checks <typeparamref name="T"/> (and its subtypes) for required properties, by C# property name.
/// </summary>
public class RequiredPropertiesRule<T> : IRichResultRule where T : Thing
{
    private readonly string[] _required;
    private readonly string[][] _requireAny;

    /// <param name="required">Properties that must all be set.</param>
    /// <param name="requireAny">Groups of properties where at least one per group must be set.</param>
    public RequiredPropertiesRule(string[] required, params string[][] requireAny)
    {
        _required = required;
        _requireAny = requireAny;
    }

    public bool AppliesTo(Thing node) => node is T;

    public IEnumerable<string> Validate(Thing node)
    {
        foreach (var property in _required.Where(p => !SchemaNodeMerger.IsSet(node, p)))
        {
            yield return $"{node.Type} is missing required property '{property}'.";
        }

        foreach (var group in _requireAny.Where(g => !g.Any(p => SchemaNodeMerger.IsSet(node, p))))
        {
            yield return $"{node.Type} needs at least one of: {string.Join(", ", group)}.";
        }
    }
}

internal static class DefaultRichResultRules
{
    public static IEnumerable<IRichResultRule> Create() => new IRichResultRule[]
    {
        new RequiredPropertiesRule<Article>(new[] { nameof(Article.Headline), nameof(Article.Image), nameof(Article.DatePublished), nameof(Article.Author) }),
        new RequiredPropertiesRule<Product>(new[] { nameof(Product.Name) }, new[] { nameof(Product.Offers), nameof(Product.Review), nameof(Product.AggregateRating) }),
        new RequiredPropertiesRule<FAQPage>(new[] { nameof(FAQPage.MainEntity) }),
        new RequiredPropertiesRule<Question>(new[] { nameof(Question.Name), nameof(Question.AcceptedAnswer) }),
        new RequiredPropertiesRule<LocalBusiness>(new[] { nameof(LocalBusiness.Name), nameof(LocalBusiness.Address) }),
        new RequiredPropertiesRule<BreadcrumbList>(new[] { nameof(BreadcrumbList.ItemListElement) }),
        new RequiredPropertiesRule<Event>(new[] { nameof(Event.Name), nameof(Event.StartDate), nameof(Event.Location) }),
    };
}
