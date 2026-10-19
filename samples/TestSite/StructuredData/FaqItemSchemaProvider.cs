using Schema.NET;
using Umbraco.Cms.Web.Common.PublishedModels;
using Webwonders.Baseline.StructuredData.Abstractions;

namespace Webwonders.StructuredData.TestSite.StructuredData;

/// <summary>
/// Turns every FAQ Item block into a Question on one FAQPage node.
/// Only runs for the block properties mapped to "FaqItem" under Webwonders:StructuredData:BlockProperties in appsettings.
/// </summary>
public class FaqItemSchemaProvider : ElementSchemaProvider<FaqItem>
{
    protected override Task ContributeAsync(ISchemaContext context, FaqItem item, SchemaBlock block, CancellationToken cancellationToken)
    {
        var faqPage = context.Graph.GetOrAdd<FAQPage>(context.Ids.ForCurrentPage("faq"));

        faqPage.MainEntity = faqPage.MainEntity
            .Append(new Question
            {
                Name = item.Question,
                AcceptedAnswer = new Answer { Text = item.Answer },
            })
            .ToList();

        return Task.CompletedTask;
    }
}
