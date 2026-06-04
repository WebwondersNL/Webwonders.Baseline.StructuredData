using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;
using Webwonders.Baseline.StructuredData.Interfaces;

namespace Webwonders.Baseline.StructuredData.ViewComponents;

[ViewComponent(Name = "StructuredDataBreadCrumb")]
public class StructuredDataBreadCrumbViewComponent(IStructuredDataService structuredDataService) : ViewComponent
{
    public HtmlString Invoke(IPublishedContent currentPage)
    {
        var json = structuredDataService.GetBreadcrumbSchema(currentPage);

        return new HtmlString($"<script type=\"application/ld+json\">{json}</script>");
    }
}

[ViewComponent(Name = "StructuredDataOrganization")]
public class StructuredDataOrganizationViewComponent(IStructuredDataService structuredDataService) : ViewComponent
{
    public HtmlString Invoke()
    {
        var json = structuredDataService.GetOrganizationSchema();

        return string.IsNullOrEmpty(json)
            ? new HtmlString(string.Empty)
            : new HtmlString($"<script type=\"application/ld+json\">{json}</script>");
    }
}

[ViewComponent(Name = "StructuredDataWebSite")]
public class StructuredDataWebSiteViewComponent(IStructuredDataService structuredDataService) : ViewComponent
{
    public HtmlString Invoke()
    {
        var json = structuredDataService.GetWebSiteSchema();

        return string.IsNullOrEmpty(json)
            ? new HtmlString(string.Empty)
            : new HtmlString($"<script type=\"application/ld+json\">{json}</script>");
    }
}

[ViewComponent(Name = "StructuredDataWebPage")]
public class StructuredDataWebPageViewComponent(IStructuredDataService structuredDataService) : ViewComponent
{
    public HtmlString Invoke(IPublishedContent currentPage)
    {
        var json = structuredDataService.GetWebPageSchema(currentPage);

        return new HtmlString($"<script type=\"application/ld+json\">{json}</script>");
    }
}