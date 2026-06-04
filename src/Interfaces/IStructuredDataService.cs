using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Webwonders.Baseline.StructuredData.Models.Base;

namespace Webwonders.Baseline.StructuredData.Interfaces;

public interface IStructuredDataService
{
    public Task<string> BuildSchemaAsync(IPublishedContent content);
    public Task<string> BuildSchemaAsync(int contentId);

    public string GetFaqSchema(List<IPublishedElement> faqItems);

    public string GetBreadcrumbSchema(IPublishedContent currentPage);

    public string GetOrganizationSchema();

    public string GetWebSiteSchema();

    public string GetWebPageSchema(IPublishedContent currentPage);

    public string GetFaqPageSchema(List<IPublishedElement> faqItems);

    /// <summary>
    /// Serializes any schema model to JSON-LD using the shared converter/options. Lets a project
    /// map its own content into the package schema models (e.g. BlogPosting, NewsArticle, VideoObject)
    /// and serialize them consistently.
    /// </summary>
    public string GetSchema(SchemaEntity entity);
}

