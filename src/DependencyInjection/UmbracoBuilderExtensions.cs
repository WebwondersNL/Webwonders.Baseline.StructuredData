using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Webwonders.Baseline.StructuredData.Abstractions;
using Webwonders.Baseline.StructuredData.Caching;
using Webwonders.Baseline.StructuredData.Configuration;
using Webwonders.Baseline.StructuredData.Core;
using Webwonders.Baseline.StructuredData.Interfaces;
using Webwonders.Baseline.StructuredData.Services;
using Webwonders.Baseline.StructuredData.Validation;

namespace Webwonders.Baseline.StructuredData.DependencyInjection;

public static class UmbracoBuilderExtensions
{
    /// <summary>Content providers. Use <c>Append&lt;T&gt;()</c> to add your own, <c>Remove&lt;T&gt;()</c> to drop a default.</summary>
    public static SchemaProviderCollectionBuilder SchemaProviders(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<SchemaProviderCollectionBuilder>();

    /// <summary>Block providers, used for the block properties mapped in appsettings.</summary>
    public static ElementSchemaProviderCollectionBuilder ElementSchemaProviders(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<ElementSchemaProviderCollectionBuilder>();

    public static SchemaGraphProcessorCollectionBuilder SchemaGraphProcessors(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<SchemaGraphProcessorCollectionBuilder>();

    /// <summary>
    /// Registers the structured data services. Called by the package's composer, so you only need this
    /// to configure options from code. Safe to call more than once.
    /// </summary>
    public static IUmbracoBuilder AddStructuredData(this IUmbracoBuilder builder, Action<StructuredDataOptions>? configure = null)
    {
        if (configure is not null)
        {
            // PostConfigure so code always wins over appsettings, whichever composer runs first.
            builder.Services.PostConfigure(configure);
        }

        if (builder.Services.Any(s => s.ServiceType == typeof(StructuredDataMarker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<StructuredDataMarker>();
        builder.Services.AddOptions<StructuredDataOptions>().BindConfiguration(StructuredDataOptions.SectionName);

        // Make sure the collections exist even when no project adds to them.
        builder.SchemaProviders();
        builder.ElementSchemaProviders();
        builder.SchemaGraphProcessors();

        builder.Services.AddScoped<SchemaContextFactory>();
        builder.Services.AddScoped<StructuredDataCollector>();
        builder.Services.AddScoped<IStructuredDataCollector>(sp => sp.GetRequiredService<StructuredDataCollector>());
        builder.Services.AddScoped<IStructuredDataRenderer, StructuredDataRenderer>();
        builder.Services.AddSingleton<ProviderResultCache>();
        builder.Services.AddSingleton<SchemaGraphValidator>();

#pragma warning disable CS0618 // Legacy service, kept for one major.
        builder.Services.AddScoped<IStructuredDataService, StructuredDataService>();
#pragma warning restore CS0618

        builder.AddNotificationHandler<ContentCacheRefresherNotification, ClearStructuredDataCacheHandler>();

        return builder;
    }

    private sealed class StructuredDataMarker;
}
