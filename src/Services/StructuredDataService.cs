
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.UmbracoContext;
using Umbraco.Extensions;
using Webwonders.Baseline.StructuredData.Converters;
using Webwonders.Baseline.StructuredData.Interfaces;
using Webwonders.Baseline.StructuredData.Models.Base;
using Webwonders.Baseline.StructuredData.Models.Old;
using Webwonders.Baseline.StructuredData.Models.SchemaElements;
using Webwonders.Baseline.StructuredData.Models.Schemas;
using Webwonders.Baseline.StructuredData.Options;

namespace Webwonders.Baseline.StructuredData.Services;

public class StructuredDataService(IOptions<StructuredDataSettings> settings) : IStructuredDataService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new SchemaEntityConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    public async Task<string> BuildSchemaAsync(IPublishedContent content)
    {
        var root = content.Root();
        var currentDocumentAlias = content.ContentType.Alias;

        var graphSchema = new GraphSchema()
        {
            Graph = new List<SchemaEntity>()
        };
        
        return JsonSerializer.Serialize(graphSchema, _jsonSerializerOptions);
    }

    public async Task<string> BuildSchemaAsync(int contentId)
    {
        throw new NotImplementedException();
    }
    
    public string GetFaqSchema(List<IPublishedElement> faqItems)
    {
        // Create a list of Question objects
        var faqList = new List<Question>();
        foreach (var faqItem in faqItems)
        {
            var question = new Question
            {
                Name = faqItem.Value<string>("question"),
                AcceptedAnswer = new Answer
                {
                    Text = faqItem.Value<HtmlEncodedString>("answer")
                }
            };
            faqList.Add(question);
        }

        // Serialize the list to JSON
        return JsonSerializer.Serialize(faqList, _jsonSerializerOptions);
    }

    public string GetBreadcrumbSchema(IPublishedContent currentPage)
        => JsonSerializer.Serialize(BuildBreadcrumb(currentPage), _jsonSerializerOptions);

    private static BreadcrumbList BuildBreadcrumb(IPublishedContent currentPage) => new()
    {
        ItemListElement = currentPage.AncestorsOrSelf()
            .OrderBy(page => page.Level)
            .Select((page, index) => new ItemListElement
            {
                Position = index + 1,
                Name = page.Name,
                Item = page.Url(mode: UrlMode.Absolute)
            })
            .ToList()
    };

    public string GetOrganizationSchema()
    {
        var org = settings.Value.Organization;
        if (org is null)
        {
            return string.Empty;
        }

        var organization = new Organization
        {
            Name = org.Name,
            Url = org.Url,
            Description = org.Description,
            Email = org.Email,
            Telephone = org.Telephone,
            SameAs = org.SameAs,
            Logo = string.IsNullOrWhiteSpace(org.Logo) ? null : new ImageObject { Url = org.Logo },
            Address = org.Address is null
                ? null
                : new PostalAddress
                {
                    StreetAddress = org.Address.StreetAddress,
                    AddressLocality = org.Address.AddressLocality,
                    PostalCode = org.Address.PostalCode,
                    AddressCountry = org.Address.AddressCountry
                }
        };

        return JsonSerializer.Serialize(organization, _jsonSerializerOptions);
    }

    public string GetWebSiteSchema()
    {
        var site = settings.Value.WebSite;
        if (site is null)
        {
            return string.Empty;
        }

        var website = new WebSite
        {
            Name = site.Name,
            Url = site.Url,
            InLanguage = site.InLanguage,
            PotentialAction = string.IsNullOrWhiteSpace(site.SearchUrlTemplate)
                ? null
                : new SearchAction { Target = site.SearchUrlTemplate }
        };

        return JsonSerializer.Serialize(website, _jsonSerializerOptions);
    }

    public string GetWebPageSchema(IPublishedContent currentPage)
    {
        var webPage = new WebPage
        {
            Url = currentPage.Url(mode: UrlMode.Absolute),
            Name = currentPage.Name,
            InLanguage = currentPage.GetCultureFromDomains(),
            Breadcrumb = BuildBreadcrumb(currentPage)
        };

        return JsonSerializer.Serialize(webPage, _jsonSerializerOptions);
    }

    public string GetFaqPageSchema(List<IPublishedElement> faqItems)
    {
        var faqPage = new FAQPage
        {
            MainEntity = faqItems.Select(faqItem => new Question
            {
                Name = faqItem.Value<string>("question"),
                AcceptedAnswer = new Answer
                {
                    Text = faqItem.Value<HtmlEncodedString>("answer")
                }
            }).ToList()
        };

        return JsonSerializer.Serialize(faqPage, _jsonSerializerOptions);
    }

    public string GetSchema(SchemaEntity entity)
        => JsonSerializer.Serialize(entity, _jsonSerializerOptions);
}