# Webwonders.Baseline.StructuredData

Renders schema.org JSON-LD for Umbraco 17 sites. The package handles collecting nodes into one `@graph`, linking them by `@id`, merging, safe output, caching and validation. **Your project decides which schema appears on which pages and where the values come from.** The package makes no assumptions about your document types or property aliases.

Schema types come from [Schema.NET](https://github.com/RehanSaeed/Schema.NET), so every schema.org type is available strongly typed.

For a working example, see [`samples/TestSite`](samples/TestSite/README.md): a small Umbraco site that uses the package the way a project would, with a panel showing the generated JSON-LD on every page.

## Setup

Install the package. It registers itself through a composer. Then render the tag in your layout:

```cshtml
@* _ViewImports.cshtml *@
@addTagHelper *, Webwonders.Baseline.StructuredData

@* Layout *@
<head>
    <structured-data />
</head>
```

Or, without tag helpers: `@await Component.InvokeAsync("StructuredData")`.

Out of the box you get a `BreadcrumbList` and a `WebPage` for every page. Both are built from node names and URLs only.

## Adding schema for a document type

Derive from `SchemaProvider<T>`, where `T` is a ModelsBuilder model **or a composition interface**:

```csharp
public class ArticleSchemaProvider : SchemaProvider<BlogPost>
{
    protected override Task ContributeAsync(ISchemaContext ctx, BlogPost post, CancellationToken ct)
    {
        ctx.Graph.Add(new Article
        {
            Id = ctx.Ids.ForCurrentPage("article"),
            Headline = post.Title,
            DatePublished = post.PublishDate,
            Author = new Person { Name = post.AuthorName },
            Publisher = SchemaRef.To<Organization>(ctx.Ids.ForSite("organization")),
            MainEntityOfPage = SchemaRef.To<WebPage>(ctx.Ids.ForCurrentPage(WebPageProvider.Fragment)),
        });
        return Task.CompletedTask;
    }
}

public class StructuredDataComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.SchemaProviders().Append<ArticleSchemaProvider>();
        builder.ElementSchemaProviders().Append<FaqSchemaProvider>();
        builder.SchemaGraphProcessors().Append<MyProcessor>();
    }
}
```

Use `SchemaProvider<IPublishedContent>` for something on every page, such as the site's Organization. Providers can inject services, and the collections are scoped.

## Adding schema for blocks

Map block properties to element provider keys in appsettings. Only mapped properties are walked. Nested block properties (and block grid areas) are only walked when their alias is mapped as well.

```json
"Webwonders": {
  "StructuredData": {
    "BlockProperties": {
      "contentBlocks": [ "Faq", "Review" ],
      "mainGrid": [ "*" ]
    }
  }
}
```

A provider's key is its class name without the `ElementSchemaProvider`/`SchemaProvider` suffix (`FaqSchemaProvider` → `Faq`), or you can override `Key`. Use `GetOrAdd` to combine several blocks into one node:

```csharp
public class FaqSchemaProvider : ElementSchemaProvider<FaqItem>
{
    protected override Task ContributeAsync(ISchemaContext ctx, FaqItem item, SchemaBlock block, CancellationToken ct)
    {
        var faq = ctx.Graph.GetOrAdd<FAQPage>(ctx.Ids.ForCurrentPage("faq"));
        faq.MainEntity = faq.MainEntity.Append(new Question
        {
            Name = item.Question,
            AcceptedAnswer = new Answer { Text = item.Answer?.ToHtmlString() },
        }).ToList();
        return Task.CompletedTask;
    }
}
```

## How the graph is built

1. **Content providers**, in ascending `Order` (the defaults use -1100 and -1000).
2. **Element providers**, for each block in the mapped properties.
3. **Collector**: `IStructuredDataCollector` contributions from controllers and views.
4. **Processors** (`ISchemaGraphProcessor`): see the whole graph and can change, remove or add anything.
5. **Validation**: in Development, logs warnings for missing required properties.

**Same `@id` = same node.** Adding a node whose id already exists merges the two: values set on the later node win, and empty values are kept from the earlier one. So you can extend a default by adding a node with its id, e.g. `ctx.Ids.ForCurrentPage(WebPageProvider.Fragment)`. When the types differ (say `Organization` → `LocalBusiness`), the later type wins and compatible values carry over.

A provider that throws is logged and skipped, so the rest of the page still renders.

## Placement and the collector

You decide where the tag goes. Anything a view adds through `IStructuredDataCollector` only appears if that view renders **before** the tag. If partials need to contribute, place the tag at the end of `<body>`. Google reads JSON-LD there as well.

## Caching

Set `Cacheable => true` on a provider to cache its nodes per page, culture and host. The cache is cleared whenever content is published and is never used in preview. A cacheable provider runs against an empty graph, so it can't depend on nodes added by other providers. Collector contributions and element providers are never cached.

Only cache providers that do real work, such as database queries, external API calls or walking large parts of the content tree. Cached nodes are deserialized on every read, so they can't be changed by a later step. For a provider that only reads a few properties, that costs about as much as running it.

For reference, on a small page in the sample site the whole pipeline (5 nodes, including blocks) adds roughly 0.6–0.8 ms per request.

## Options

```json
"Webwonders": {
  "StructuredData": {
    "Enabled": true,
    "Defaults": { "BreadcrumbList": true, "WebPage": true },
    "BlockProperties": { },
    "MaxBlockDepth": 5,
    "Cache": { "Enabled": true, "Duration": "01:00:00" },
    "Validation": { "WarnInDevelopment": true },
    "WriteIndented": false
  }
}
```

Options set in code with `builder.AddStructuredData(o => ...)` take precedence over appsettings. To drop a default entirely: `builder.SchemaProviders().Remove<BreadcrumbListProvider>()`.

## Testing providers

`SchemaContext` has a public constructor, so providers can be unit tested without starting Umbraco:

```csharp
var context = new SchemaContext(page, new[] { home, page }, c => new Uri("https://site.nl" + c.UrlSegment));
await new ArticleSchemaProvider().ContributeAsync(context, CancellationToken.None);
```

## Gotchas

- `Schema.NET.Constants` clashes with `Umbraco.Cms.Core.Constants` when both namespaces are imported. Fully qualify the one you need.
- Don't compare `@id` values with `Uri.Equals`, because it ignores the `#fragment`. The graph compares the full id string.
- Schema.NET's implicit conversions (`string` → `OneOrMany<string>`) aren't annotated for null, so assigning optional Umbraco properties gives CS8604 warnings. Null and empty values are simply treated as "not set", so it's safe to silence CS8604 for your structured data folder with an `.editorconfig` (see the sample).
- Schema.NET writes a `DateTime` as a date only (`"2026-10-01"`). Use a `DateTimeOffset` when the time and time zone matter, e.g. for `datePublished`.
- Content created with `IContentService` doesn't get the document type's default template. Set `TemplateId` yourself, or the page won't render.

## Legacy

`IStructuredDataService` and the `StructuredDataBreadCrumb` / `StructuredDataOrganization` view components are obsolete and will be removed in the next major version.
