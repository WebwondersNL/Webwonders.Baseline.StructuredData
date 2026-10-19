using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;
using Schema.NET;
using Webwonders.Baseline.StructuredData.Abstractions;

namespace Webwonders.Baseline.StructuredData.Core;

public static class JsonLdSerializer
{
    private const string ContextProperty = "@context";

    // JavaScriptEncoder.Create always escapes HTML-sensitive characters (<, >, &, ', "), so content
    // can never close the script tag. Allowing all ranges keeps non-ASCII text readable.
    private static readonly JavaScriptEncoder SafeEncoder = JavaScriptEncoder.Create(UnicodeRanges.All);

    private static readonly JsonSerializerOptions Compact = CreateOptions(writeIndented: false);
    private static readonly JsonSerializerOptions Indented = CreateOptions(writeIndented: true);

    /// <summary>
    /// Serializes the graph as <c>{ "@context": "https://schema.org", "@graph": [...] }</c>, or returns null when it is empty.
    /// </summary>
    public static string? Serialize(ISchemaGraph graph, bool writeIndented = false)
    {
        if (graph.Nodes.Count == 0)
        {
            return null;
        }

        var document = new Dictionary<string, object>
        {
            [ContextProperty] = Constants.SchemaData.SchemaOrgContext,
            // Declared as object so each node serializes with its runtime type instead of Thing.
            ["@graph"] = graph.Nodes.Cast<object>().ToList(),
        };

        return JsonSerializer.Serialize(document, writeIndented ? Indented : Compact);
    }

    public static string ToScriptTag(string? json)
        => string.IsNullOrEmpty(json)
            ? string.Empty
            : $"<script type=\"application/ld+json\">{json}</script>";

    internal static string SerializeNode(Thing node) => JsonSerializer.Serialize(node, node.GetType(), Compact);

    internal static Thing? DeserializeNode(string json, Type type) => (Thing?)JsonSerializer.Deserialize(json, type, Compact);

    /// <summary>
    /// Schema.NET writes @context on every object; only the document root should have it.
    /// Dropping it from the contract avoids post-processing the output.
    /// </summary>
    private static void RemoveContext(JsonTypeInfo typeInfo)
    {
        if (!typeof(JsonLdObject).IsAssignableFrom(typeInfo.Type))
        {
            return;
        }

        var context = typeInfo.Properties.FirstOrDefault(p => p.Name == ContextProperty);
        if (context is not null)
        {
            typeInfo.Properties.Remove(context);
        }
    }

    private static JsonSerializerOptions CreateOptions(bool writeIndented) => new()
    {
        // Matches Schema.NET's own settings: unset OneOrMany/Values structs are skipped.
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Encoder = SafeEncoder,
        WriteIndented = writeIndented,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { RemoveContext } },
    };
}
