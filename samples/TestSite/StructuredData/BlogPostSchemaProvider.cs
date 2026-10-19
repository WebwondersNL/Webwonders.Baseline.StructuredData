using Schema.NET;
using Umbraco.Cms.Web.Common.PublishedModels;
using Webwonders.Baseline.StructuredData.Abstractions;
using Webwonders.Baseline.StructuredData.Defaults;

namespace Webwonders.StructuredData.TestSite.StructuredData;

/// <summary>
/// A BlogPosting for blog posts, linked to the page and the organization by @id.
/// </summary>
public class BlogPostSchemaProvider : SchemaProvider<BlogPost>
{
    protected override Task ContributeAsync(ISchemaContext context, BlogPost post, CancellationToken cancellationToken)
    {
        context.Graph.Add(new BlogPosting
        {
            Id = context.Ids.ForCurrentPage("article"),
            Headline = post.Name,
            Description = post.Excerpt,
            DatePublished = post.PublishDate,
            DateModified = post.UpdateDate,
            Author = new Person { Name = post.AuthorName },
            Publisher = SchemaRef.To<Organization>(context.Ids.ForSite(OrganizationSchemaProvider.Fragment)),
            MainEntityOfPage = SchemaRef.To<WebPage>(context.Ids.ForCurrentPage(WebPageProvider.Fragment)),
        });

        return Task.CompletedTask;
    }
}
