using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Umbraco.Cms.Core.Models.PublishedContent;
using Webwonders.Baseline.StructuredData.Abstractions;

namespace Webwonders.Baseline.StructuredData.Rendering;

/// <summary>
/// The view component alternative to <see cref="StructuredDataTagHelper"/>:
/// <c>@await Component.InvokeAsync("StructuredData")</c>.
/// </summary>
[ViewComponent(Name = "StructuredData")]
public class StructuredDataViewComponent(IStructuredDataRenderer renderer) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(IPublishedContent? content = null)
        => new HtmlContentViewComponentResult(new HtmlString(await renderer.RenderScriptAsync(content, HttpContext.RequestAborted)));
}
