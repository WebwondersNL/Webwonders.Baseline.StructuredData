using Schema.NET;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;
using Webwonders.Baseline.StructuredData.Abstractions;

namespace Webwonders.StructuredData.TestSite.StructuredData;

/// <summary>
/// The company behind the site, on every page. The details live on the Home node in this project;
/// another project might use a settings node, appsettings or an external API instead.
/// </summary>
public class OrganizationSchemaProvider : SchemaProvider<IPublishedContent>
{
    public const string Fragment = "organization";

    // Same output for every page of a site, so cache it until something is published.
    public override bool Cacheable => true;

    protected override bool AppliesTo(ISchemaContext context, IPublishedContent content) => context.Root is Home;

    protected override Task ContributeAsync(ISchemaContext context, IPublishedContent content, CancellationToken cancellationToken)
    {
        var home = (Home)context.Root;

        context.Graph.Add(new Organization
        {
            Id = context.Ids.ForSite(Fragment),
            Name = home.CompanyName,
            Url = context.AbsoluteUrl(home),
            Email = home.Email,
            Telephone = home.Telephone,
            Address = new PostalAddress
            {
                StreetAddress = home.StreetAddress,
                PostalCode = home.PostalCode,
                AddressLocality = home.City,
                AddressCountry = home.Country,
            },
        });

        return Task.CompletedTask;
    }
}
