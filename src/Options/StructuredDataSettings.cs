namespace Webwonders.Baseline.StructuredData.Options;

public class StructuredDataSettings
{
    public const string ConfigurationName = "Webwonders:StructuredData";

    public OrganizationSettings? Organization { get; set; }
    public WebSiteSettings? WebSite { get; set; }
}

public class WebSiteSettings
{
    public string? Name { get; set; }
    public string? Url { get; set; }
    public string? InLanguage { get; set; }

    /// <summary>
    /// Search URL template for the sitelinks search box, e.g.
    /// "https://example.com/search?q={search_term_string}". When null, no SearchAction is emitted.
    /// </summary>
    public string? SearchUrlTemplate { get; set; }
}

public class OrganizationSettings
{
    public string? Name { get; set; }
    public string? Url { get; set; }
    public string? Logo { get; set; }
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? Telephone { get; set; }
    public string[]? SameAs { get; set; }
    public OrganizationAddressSettings? Address { get; set; }
}

public class OrganizationAddressSettings
{
    public string? StreetAddress { get; set; }
    public string? AddressLocality { get; set; }
    public string? PostalCode { get; set; }
    public string? AddressCountry { get; set; }
}
