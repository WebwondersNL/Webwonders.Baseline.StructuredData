using Microsoft.AspNetCore.Razor.TagHelpers;
using Umbraco.Cms.Core.Models.PublishedContent;
using Webwonders.Baseline.StructuredData.Abstractions;

namespace Webwonders.Baseline.StructuredData.Rendering;

/// <summary>
/// Renders the page's JSON-LD: <c>&lt;structured-data /&gt;</c>.
/// Add <c>@addTagHelper *, Webwonders.Baseline.StructuredData</c> to your <c>_ViewImports.cshtml</c>.
/// </summary>
/// <remarks>
/// Place it wherever you like. Nodes added from views through <see cref="IStructuredDataCollector"/>
/// only end up in the output when those views render before this tag, e.g. at the end of <c>&lt;body&gt;</c>.
/// </remarks>
[HtmlTargetElement("structured-data", TagStructure = TagStructure.WithoutEndTag)]
public class StructuredDataTagHelper(IStructuredDataRenderer renderer) : TagHelper
{
    /// <summary>The page to render structured data for. Defaults to the current page.</summary>
    public IPublishedContent? Content { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        output.Content.SetHtmlContent(await renderer.RenderScriptAsync(Content));
    }
}
