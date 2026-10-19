using Umbraco.Cms.Core.Models.PublishedContent;

namespace Webwonders.Baseline.StructuredData.Interfaces;

[Obsolete("Use IStructuredDataRenderer. This service will be removed in the next major version.")]
public interface IStructuredDataService
{
    public Task<string> BuildSchemaAsync(IPublishedContent content);
    public Task<string> BuildSchemaAsync(int contentId);

    [Obsolete("Relies on the 'question' and 'answer' aliases. Write an ElementSchemaProvider for your FAQ block instead.")]
    public string GetFaqSchema(List<IPublishedElement> faqItems);
}
