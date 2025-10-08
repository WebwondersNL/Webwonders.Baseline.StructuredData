using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Webwonders.Baseline.StructuredData.ViewComponents;

[Obsolete("Soon to be completely replaced, this is here purely for legacy reasons.")]
[ViewComponent(Name = "StructuredDataBreadCrumb")]
public class StructuredDataBreadCrumbViewComponent() : ViewComponent
{
    public HtmlString Invoke(IPublishedContent currentPage)
    {
        var breadCrumbCollection = currentPage.AncestorsOrSelf().ToList();

        var returnString = new StringBuilder();
        returnString.AppendLine("<script type=\"application/ld+json\">");
        returnString.AppendLine("{");
        returnString.AppendLine("  \"@context\": \"https://schema.org\",");
        returnString.AppendLine("  \"@type\": \"BreadcrumbList\",");
        returnString.AppendLine("  \"itemListElement\": [");

        foreach (var page in breadCrumbCollection.OrderBy(x => x.Level))
        {
            returnString.AppendLine("  \"itemListElement\": {");
            returnString.AppendLine("    \"@type\": \"ListItem\",");
            returnString.AppendLine($"    \"position\": \"{page.Level}\",");
            returnString.AppendLine($"    \"name\": \"{page.Name}\",");
            returnString.AppendLine($"    \"item\": \"{page.Url()}\"");
            if (page.Id == breadCrumbCollection.Last().Id)
            {
                returnString.AppendLine("  }");
            }
            else
            {
                returnString.AppendLine("  },");
            }
        }

        returnString.AppendLine(" ]");
        returnString.AppendLine("}");
        returnString.AppendLine("</script>");

        return new HtmlString(returnString.ToString());
    }
}

[Obsolete("Soon to be completely replaced, this is here purely for legacy reasons.")]
[ViewComponent(Name = "StructuredDataOrganization")]
public class StructuredDataOrganization
{
    public HtmlString Invoke(IPublishedContent currentPage)
    {
        var home = currentPage.AncestorOrSelf("home")!;
        var returnString = new StringBuilder();
        returnString.AppendLine("<script type=\"application/ld+json\">");
        returnString.AppendLine("{");
        returnString.AppendLine("\"@context\": \"https://schema.org\",");
        returnString.AppendLine("\"@type\": \"Organization\",");
        returnString.AppendLine("\"image\": \"https://www.webwonders.nl/images/images/backoffice/login4.jpg\",");
        returnString.AppendLine("\"url\": \"https://www.webwonders.nl\",");
        returnString.AppendLine("\"logo\": \"https://www.webwonders.nl/images/backoffice/Webwonders.svg\",");
        returnString.AppendLine("\"name\": \"Webwonders\",");
        returnString.AppendLine($"\"description\": \"{home.Value("seoDescription")}\",");
        returnString.AppendLine("\"email\": \"info@webwonders.nl\",");
        returnString.AppendLine("\"telephone\": \"+31 40 8200393\",");
        returnString.AppendLine("\"address\": {");
        returnString.AppendLine("\"@type\": \"PostalAddress\",");
        returnString.AppendLine("\"streetAddress\": \"Klokgebouw 262\",");
        returnString.AppendLine("\"addressLocality\": \"Eindhoven\",");
        returnString.AppendLine("\"addressCountry\": \"NL\",");
        returnString.AppendLine("\"addressRegion\": \"Noord-Brabant\",");
        returnString.AppendLine("\"postalCode\": \"5617AC\"");
        returnString.AppendLine("},");
        returnString.AppendLine("}");
        returnString.AppendLine("</script>");

        return new HtmlString(returnString.ToString());
    }
}