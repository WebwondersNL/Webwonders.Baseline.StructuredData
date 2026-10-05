using System.Text.Json;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Webwonders.Baseline.StructuredData.ViewComponents;

internal static class LegacyJsonLd
{
    // System.Text.Json's default encoder escapes <, > and &, so values can't break out of the script tag.
    public static HtmlString ToScript(object schema)
    {
        var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        return new HtmlString($"<script type=\"application/ld+json\">\n{json}\n</script>\n");
    }
}

[Obsolete("Soon to be completely replaced, this is here purely for legacy reasons.")]
[ViewComponent(Name = "StructuredDataBreadCrumb")]
public class StructuredDataBreadCrumbViewComponent() : ViewComponent
{
    public HtmlString Invoke(IPublishedContent currentPage)
    {
        var breadCrumbCollection = currentPage.AncestorsOrSelf().OrderBy(x => x.Level).ToList();

        var schema = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "BreadcrumbList",
            ["itemListElement"] = breadCrumbCollection.Select((page, index) => new Dictionary<string, object?>
            {
                ["@type"] = "ListItem",
                ["position"] = index + 1,
                ["name"] = page.Name,
                ["item"] = page.Url(mode: UrlMode.Absolute)
            }).ToList()
        };

        return LegacyJsonLd.ToScript(schema);
    }
}

[Obsolete("Soon to be completely replaced, this is here purely for legacy reasons.")]
[ViewComponent(Name = "StructuredDataOrganization")]
public class StructuredDataOrganization
{
    public HtmlString Invoke(IPublishedContent currentPage)
    {
        var home = currentPage.AncestorOrSelf("home")!;

        var schema = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Organization",
            ["image"] = "https://www.webwonders.nl/images/images/backoffice/login4.jpg",
            ["url"] = "https://www.webwonders.nl",
            ["logo"] = "https://www.webwonders.nl/images/backoffice/Webwonders.svg",
            ["name"] = "Webwonders",
            ["description"] = home.Value<string>("seoDescription"),
            ["email"] = "info@webwonders.nl",
            ["telephone"] = "+31 40 8200393",
            ["address"] = new Dictionary<string, object?>
            {
                ["@type"] = "PostalAddress",
                ["streetAddress"] = "Klokgebouw 262",
                ["addressLocality"] = "Eindhoven",
                ["addressCountry"] = "NL",
                ["addressRegion"] = "Noord-Brabant",
                ["postalCode"] = "5617AC"
            }
        };

        return LegacyJsonLd.ToScript(schema);
    }
}
