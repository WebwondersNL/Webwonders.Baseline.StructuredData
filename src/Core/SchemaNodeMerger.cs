using System.Collections.Concurrent;
using System.Reflection;
using Schema.NET;

namespace Webwonders.Baseline.StructuredData.Core;

/// <summary>
/// Merges two Schema.NET nodes that share an <c>@id</c>.
/// </summary>
internal static class SchemaNodeMerger
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    /// <summary>
    /// Merges <paramref name="incoming"/> into <paramref name="existing"/> and returns the node that should live in the graph.
    /// Same type: values set on the incoming node overwrite the existing node, which keeps its identity.
    /// Different type: the incoming node wins, and values it leaves empty are filled from compatible properties of the existing node.
    /// </summary>
    public static Thing Merge(Thing existing, Thing incoming)
    {
        if (ReferenceEquals(existing, incoming))
        {
            return existing;
        }

        if (existing.GetType() == incoming.GetType())
        {
            CopySetValues(incoming, existing, overwrite: true);
            return existing;
        }

        CopySetValues(existing, incoming, overwrite: false);
        return incoming;
    }

    public static bool IsSet(object? value) => value switch
    {
        null => false,
        IValues values => values.Count > 0,
        string text => text.Length > 0,
        _ => true,
    };

    public static bool IsSet(Thing node, string propertyName)
    {
        var property = GetProperties(node.GetType()).FirstOrDefault(p => p.Name == propertyName);
        return property is not null && IsSet(property.GetValue(node));
    }

    private static void CopySetValues(Thing source, Thing target, bool overwrite)
    {
        var targetProperties = GetProperties(target.GetType()).ToDictionary(p => p.Name);

        foreach (var sourceProperty in GetProperties(source.GetType()))
        {
            if (!targetProperties.TryGetValue(sourceProperty.Name, out var targetProperty)
                || targetProperty.PropertyType != sourceProperty.PropertyType)
            {
                continue;
            }

            var value = sourceProperty.GetValue(source);
            if (!IsSet(value) || (!overwrite && IsSet(targetProperty.GetValue(target))))
            {
                continue;
            }

            targetProperty.SetValue(target, value);
        }
    }

    private static PropertyInfo[] GetProperties(Type type) => PropertyCache.GetOrAdd(type, t => t
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
        .Where(p => p.Name is not (nameof(JsonLdObject.Id) or nameof(JsonLdObject.Context)))
        .ToArray());
}
