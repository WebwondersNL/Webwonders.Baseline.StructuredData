# Structured data test site

An Umbraco 17 site that uses the package the way a project would after installing it. Every page shows a panel with the JSON-LD rendered in `<head>` and the provider that added each node.

## Run it

```bash
cd samples/TestSite
dotnet run
```

On first start, the site installs itself (SQLite) and creates the document types and demo content. Then open **https://localhost:44322** (from `Properties/launchSettings.json`).

- Backoffice: https://localhost:44322/umbraco, log in with `admin@example.com` / `TestSite!12345` (local only). The backoffice login only works over HTTPS.
- To start over, stop the site and delete `umbraco/Data`.

## What to look at

| Page | Shows |
|---|---|
| Home | `Organization` and `WebSite` from the project, `WebPage` and `BreadcrumbList` from the package defaults |
| Blog → post | `BlogPosting` from `SchemaProvider<BlogPost>` |
| Frequently asked questions | One `FAQPage` built from three FAQ blocks by `ElementSchemaProvider<FaqItem>` |

Things to try:

- Edit the company details on the Home node and publish. The cached Organization updates on every page.
- Add a FAQ item block and publish.
- Check the console: in Development the package warns that the `BlogPosting` is missing an `image`.
- Paste the JSON-LD (use the copy button) into [validator.schema.org](https://validator.schema.org/).

## Where things are

**What a developer writes (`StructuredData/`):**

| File | Uses |
|---|---|
| `StructuredDataComposer.cs` | Registers everything below |
| `OrganizationSchemaProvider.cs` | A cacheable provider for every page, reading from the Home node |
| `WebSiteSchemaProvider.cs` | Adds something the package deliberately doesn't ship |
| `PageMetaSchemaProvider.cs` | Matches a **composition** (`IPageMeta`) and merges into the default WebPage by `@id` |
| `BlogPostSchemaProvider.cs` | A provider for one document type, referencing other nodes with `SchemaRef` |
| `FaqItemSchemaProvider.cs` | A block provider that aggregates into one node with `GetOrAdd` |
| `LinkPageToSiteProcessor.cs` | A processor linking nodes after all providers have run |
| `appsettings.json` | `Webwonders:StructuredData:BlockProperties` maps `contentBlocks` to `FaqItem` |
| `Views/master.cshtml` | `<structured-data />` in `<head>` |

`StructuredData/.editorconfig` silences CS8604. Schema.NET's implicit conversions aren't annotated for null, but null or empty values are treated as "not set", so assigning optional Umbraco properties directly is fine.

**Test-site scaffolding (not package usage):**

- `Seeding/`: creates document types and content on first start. A real project does this in the backoffice.
- `Views/Partials/StructuredDataPanel.cshtml`: the debug panel.
- `umbraco/models/`: ModelsBuilder models (`SourceCodeAuto` in Development), committed so the site builds straight after cloning.
