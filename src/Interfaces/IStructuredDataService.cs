using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Webwonders.Baseline.StructuredData.Interfaces;

public interface IStructuredDataService
{
    public Task<string> BuildSchemaAsync(IPublishedContent content);
    public Task<string> BuildSchemaAsync(int contentId);
    
    public string GetFaqSchema(List<IPublishedElement> faqItems);
}

