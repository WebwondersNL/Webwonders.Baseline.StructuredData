using System.Text.Json;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using UmbracoConstants = Umbraco.Cms.Core.Constants;

namespace Webwonders.StructuredData.TestSite.Seeding;

// -------------------------------------------------------------------------------------------------
// Test-site scaffolding only: creates the document types and demo content on first boot so the site
// works straight after cloning. In a real project these are made in the backoffice (or with uSync).
// Nothing in this folder is needed to use the structured data package.
// -------------------------------------------------------------------------------------------------

public class SeedingComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
        => builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, DemoContentSeeder>();
}

public class DemoContentSeeder(
    IContentTypeService contentTypeService,
    IContentService contentService,
    IDataTypeService dataTypeService,
    ITemplateService templateService,
    PropertyEditorCollection propertyEditors,
    IConfigurationEditorJsonSerializer configurationSerializer,
    IShortStringHelper shortStringHelper,
    IWebHostEnvironment webHostEnvironment,
    ILogger<DemoContentSeeder> logger) : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private static readonly Guid UserKey = UmbracoConstants.Security.SuperUserKey;

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
    {
        if (contentTypeService.Get("home") is not null)
        {
            return;
        }

        logger.LogInformation("Seeding structured data demo content");

        var textstring = await dataTypeService.GetAsync(UmbracoConstants.DataTypes.Guids.TextstringGuid);
        var textarea = await dataTypeService.GetAsync(UmbracoConstants.DataTypes.Guids.TextareaGuid);
        var datePicker = await dataTypeService.GetAsync(UmbracoConstants.DataTypes.Guids.DatePickerWithTimeGuid);

        var template = await CreateTemplateAsync();

        // Composition: every page gets a meta description.
        var pageMeta = Save(NewType("pageMeta", "Page Meta", "icon-tag"), t =>
            Add(t, textarea!, "metaDescription", "Meta description", "seo", "SEO"));

        // Element type used in the FAQ block list.
        var faqItem = Save(NewType("faqItem", "FAQ Item", "icon-help-alt", isElement: true), t =>
        {
            Add(t, textstring!, "question", "Question", "content", "Content");
            Add(t, textarea!, "answer", "Answer", "content", "Content");
        });

        var blockList = await CreateFaqBlockListAsync(faqItem);

        var blogPost = Save(NewPage("blogPost", "Blog Post", "icon-edit", template, pageMeta), t =>
        {
            Add(t, textstring!, "authorName", "Author", "content", "Content");
            Add(t, datePicker!, "publishDate", "Publish date", "content", "Content");
            Add(t, textarea!, "excerpt", "Excerpt", "content", "Content");
        });

        var blogOverview = Save(NewPage("blogOverview", "Blog Overview", "icon-newspaper", template, pageMeta), t =>
            t.AllowedContentTypes = new[] { new ContentTypeSort(blogPost.Key, 0, blogPost.Alias) });

        var contentPage = Save(NewPage("contentPage", "Content Page", "icon-document", template, pageMeta), t =>
            Add(t, blockList, "contentBlocks", "Content blocks", "content", "Content"));

        var home = Save(NewPage("home", "Home", "icon-home", template, pageMeta), t =>
        {
            t.AllowedAsRoot = true;
            t.AllowedContentTypes = new[]
            {
                new ContentTypeSort(blogOverview.Key, 0, blogOverview.Alias),
                new ContentTypeSort(contentPage.Key, 1, contentPage.Alias),
            };
            Add(t, textstring!, "companyName", "Company name", "company", "Company details");
            Add(t, textstring!, "telephone", "Telephone", "company", "Company details");
            Add(t, textstring!, "email", "Email", "company", "Company details");
            Add(t, textstring!, "streetAddress", "Street address", "company", "Company details");
            Add(t, textstring!, "postalCode", "Postal code", "company", "Company details");
            Add(t, textstring!, "city", "City", "company", "Company details");
            Add(t, textstring!, "country", "Country code", "company", "Company details");
        });

        SeedContent(home, blogOverview, blogPost, contentPage, faqItem);
    }

    private void SeedContent(IContentType home, IContentType blogOverview, IContentType blogPost, IContentType contentPage, IContentType faqItem)
    {
        var homePage = Publish(home.Alias, "Webwonders Demo", -1, c =>
        {
            c.SetValue("metaDescription", "Demo site for the Webwonders structured data package.");
            c.SetValue("companyName", "Webwonders");
            c.SetValue("telephone", "+31 40 8200393");
            c.SetValue("email", "info@webwonders.nl");
            c.SetValue("streetAddress", "Klokgebouw 262");
            c.SetValue("postalCode", "5617AC");
            c.SetValue("city", "Eindhoven");
            c.SetValue("country", "NL");
        });

        var blog = Publish(blogOverview.Alias, "Blog", homePage.Id, c =>
            c.SetValue("metaDescription", "News and articles."));

        Publish(blogPost.Alias, "Structured data without the opinions", blog.Id, c =>
        {
            c.SetValue("metaDescription", "Why our structured data package lets your project decide what to render.");
            c.SetValue("authorName", "Jane Developer");
            c.SetValue("publishDate", new DateTime(2026, 10, 1, 9, 30, 0));
            c.SetValue("excerpt", "Providers, element providers and processors: how a project plugs its own schema into the package.");
        });

        Publish(contentPage.Alias, "Frequently asked questions", homePage.Id, c =>
        {
            c.SetValue("metaDescription", "Answers to common questions.");
            c.SetValue("contentBlocks", FaqBlocks(faqItem,
                ("Do I need to use specific property aliases?", "No. Your providers decide which properties are used."),
                ("Can I change what the package renders?", "Yes, with a provider using the same @id, or a graph processor."),
                ("Where does this FAQ schema come from?", "An ElementSchemaProvider<FaqItem>, enabled for the contentBlocks property in appsettings.")));
        });
    }

    private IContent Publish(string alias, string name, int parentId, Action<IContent> setValues)
    {
        var content = contentService.Create(name, parentId, alias);

        // The backoffice assigns the default template; content created from code has to do it itself.
        content.TemplateId = contentTypeService.Get(alias)?.DefaultTemplateId;
        setValues(content);
        contentService.Save(content);
        var result = contentService.Publish(content, new[] { "*" });
        if (!result.Success)
        {
            logger.LogError("Could not publish {Name}: {Result}", name, result.Result);
        }

        return content;
    }

    /// <summary>The v17 block list value format: layout + contentData + expose.</summary>
    private static string FaqBlocks(IContentType faqItem, params (string Question, string Answer)[] items)
    {
        var blocks = items.Select(i => (Key: Guid.NewGuid(), i.Question, i.Answer)).ToList();

        return JsonSerializer.Serialize(new
        {
            layout = new Dictionary<string, object>
            {
                [UmbracoConstants.PropertyEditors.Aliases.BlockList] = blocks.Select(b => new { contentKey = b.Key }),
            },
            contentData = blocks.Select(b => new
            {
                contentTypeKey = faqItem.Key,
                key = b.Key,
                values = new[]
                {
                    new { alias = "question", value = b.Question, culture = (string?)null, segment = (string?)null },
                    new { alias = "answer", value = b.Answer, culture = (string?)null, segment = (string?)null },
                },
            }),
            settingsData = Array.Empty<object>(),
            expose = blocks.Select(b => new { contentKey = b.Key, culture = (string?)null, segment = (string?)null }),
        });
    }

    private async Task<ITemplate> CreateTemplateAsync()
    {
        // page.cshtml uses master.cshtml as its layout, so the master template has to exist first.
        await CreateTemplateAsync("Master", "master");
        return await CreateTemplateAsync("Page", "page");
    }

    private async Task<ITemplate> CreateTemplateAsync(string name, string alias)
    {
        // The views already exist in the repo; pass their content so the files stay the same.
        var path = Path.Combine(webHostEnvironment.ContentRootPath, "Views", $"{alias}.cshtml");
        var view = await System.IO.File.ReadAllTextAsync(path);
        var result = await templateService.CreateAsync(name, alias, view, UserKey);
        return result.Success
            ? result.Result
            : throw new InvalidOperationException($"Could not create template '{alias}': {result.Status}");
    }

    private async Task<IDataType> CreateFaqBlockListAsync(IContentType faqItem)
    {
        var dataType = new DataType(propertyEditors[UmbracoConstants.PropertyEditors.Aliases.BlockList]!, configurationSerializer, -1)
        {
            Name = "FAQ Block List",
            EditorUiAlias = "Umb.PropertyEditorUi.BlockList",
            ConfigurationData = new Dictionary<string, object>
            {
                ["blocks"] = new[] { new Dictionary<string, object> { ["contentElementTypeKey"] = faqItem.Key, ["label"] = "{{question}}" } },
            },
        };

        var result = await dataTypeService.CreateAsync(dataType, UserKey);
        return result.Success
            ? result.Result
            : throw new InvalidOperationException($"Could not create block list: {result.Status}");
    }

    private ContentType NewType(string alias, string name, string icon, bool isElement = false)
        => new(shortStringHelper, -1) { Alias = alias, Name = name, Icon = icon, IsElement = isElement };

    private ContentType NewPage(string alias, string name, string icon, ITemplate template, IContentType composition)
    {
        var type = NewType(alias, name, icon);
        type.AllowedTemplates = new[] { template };
        type.SetDefaultTemplate(template);
        type.AddContentType(composition);
        return type;
    }

    private void Add(ContentType type, IDataType dataType, string alias, string name, string groupAlias, string groupName)
        => type.AddPropertyType(new PropertyType(shortStringHelper, dataType, alias) { Name = name }, groupAlias, groupName);

#pragma warning disable CS0618 // The async Create/Update APIs need full models; Save is fine for seeding.
    private IContentType Save(ContentType type, Action<ContentType> configure)
    {
        configure(type);
        contentTypeService.Save(type);
        return type;
    }
#pragma warning restore CS0618
}
