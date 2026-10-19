namespace Webwonders.Baseline.StructuredData.Configuration;

/// <summary>
/// Bound from the <c>Webwonders:StructuredData</c> section of appsettings.
/// </summary>
public class StructuredDataOptions
{
    public const string SectionName = "Webwonders:StructuredData";

    /// <summary>The key in <see cref="BlockProperties"/> that matches every element provider.</summary>
    public const string AllProviders = "*";

    public bool Enabled { get; set; } = true;

    public DefaultsOptions Defaults { get; set; } = new();

    /// <summary>
    /// Maps block property aliases to the element provider keys that may handle blocks in them, e.g.
    /// <c>"contentBlocks": [ "Faq", "Review" ]</c> or <c>"mainGrid": [ "*" ]</c>.
    /// Nested block properties are only walked when their alias is mapped too.
    /// </summary>
    public Dictionary<string, string[]> BlockProperties { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>How deep nested blocks are walked.</summary>
    public int MaxBlockDepth { get; set; } = 5;

    public CacheOptions Cache { get; set; } = new();

    public ValidationOptions Validation { get; set; } = new();

    public bool WriteIndented { get; set; }

    public class DefaultsOptions
    {
        public bool BreadcrumbList { get; set; } = true;

        public bool WebPage { get; set; } = true;
    }

    public class CacheOptions
    {
        /// <summary>Allows providers that opt in with <c>Cacheable</c> to be cached.</summary>
        public bool Enabled { get; set; } = true;

        public TimeSpan Duration { get; set; } = TimeSpan.FromHours(1);
    }

    public class ValidationOptions
    {
        /// <summary>Logs warnings for missing required properties when running in Development.</summary>
        public bool WarnInDevelopment { get; set; } = true;
    }
}
