using System.Collections;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Schema.NET;
using Webwonders.Baseline.StructuredData.Abstractions;

namespace Webwonders.Baseline.StructuredData.Validation;

internal sealed class SchemaGraphValidator(IEnumerable<IRichResultRule> rules, ILogger<SchemaGraphValidator> logger)
{
    private readonly IRichResultRule[] _rules = DefaultRichResultRules.Create().Concat(rules).ToArray();

    public void Validate(ISchemaContext context)
    {
        foreach (var node in context.Graph.Nodes)
        {
            var problems = SelfAndNested(node)
                .SelectMany(n => _rules.Where(r => r.AppliesTo(n)).SelectMany(r => r.Validate(n)))
                .Distinct()
                .ToList();

            if (problems.Count == 0)
            {
                continue;
            }

            logger.LogWarning(
                "Structured data for content {ContentId} has problems on node {NodeId} (added by {Sources}): {Problems}",
                context.Content.Id,
                node.Id?.ToString() ?? node.Type,
                string.Join(", ", context.Graph.GetSources(node)),
                string.Join(" ", problems));
        }
    }

    internal static IEnumerable<Thing> SelfAndNested(Thing root)
    {
        var visited = new HashSet<Thing>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<Thing>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (!visited.Add(node))
            {
                continue;
            }

            // Reference-only nodes (just an @id) point at a node that's validated on its own.
            if (!ReferenceEquals(node, root) && IsReferenceOnly(node))
            {
                continue;
            }

            yield return node;

            foreach (var property in node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.GetIndexParameters().Length == 0 && property.GetValue(node) is IValues values)
                {
                    foreach (var child in ((IEnumerable)values).OfType<Thing>())
                    {
                        stack.Push(child);
                    }
                }
            }
        }
    }

    private static bool IsReferenceOnly(Thing node)
        => node.Id is not null
           && node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
               .Where(p => p.GetIndexParameters().Length == 0 && p.PropertyType.IsAssignableTo(typeof(IValues)))
               .All(p => ((IValues)p.GetValue(node)!).Count == 0);
}
