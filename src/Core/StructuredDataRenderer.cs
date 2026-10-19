using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models.PublishedContent;
using Webwonders.Baseline.StructuredData.Abstractions;
using Webwonders.Baseline.StructuredData.Caching;
using Webwonders.Baseline.StructuredData.Configuration;
using Webwonders.Baseline.StructuredData.DependencyInjection;
using Webwonders.Baseline.StructuredData.Validation;

namespace Webwonders.Baseline.StructuredData.Core;

internal sealed class StructuredDataRenderer(
    SchemaContextFactory contextFactory,
    SchemaProviderCollection providers,
    ElementSchemaProviderCollection elementProviders,
    SchemaGraphProcessorCollection processors,
    StructuredDataCollector collector,
    ProviderResultCache cache,
    SchemaGraphValidator validator,
    IOptionsSnapshot<StructuredDataOptions> options,
    IHostEnvironment hostEnvironment,
    ILogger<StructuredDataRenderer> logger) : IStructuredDataRenderer
{
    public async Task<ISchemaGraph?> BuildGraphAsync(IPublishedContent? content = null, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        content ??= contextFactory.GetCurrentContent();
        if (!settings.Enabled || content is null)
        {
            return null;
        }

        var context = contextFactory.Create(content);
        var graph = (SchemaGraph)context.Graph;

        await RunProvidersAsync(context, graph, settings, cancellationToken);
        await RunElementProvidersAsync(context, graph, settings, cancellationToken);

        await RunGuardedAsync(graph, nameof(IStructuredDataCollector), context, () =>
        {
            collector.ApplyTo(graph);
            return Task.CompletedTask;
        });

        foreach (var processor in processors.OrderBy(p => p.Order))
        {
            await RunGuardedAsync(graph, processor.GetType().Name, context,
                () => processor.ProcessAsync(context, cancellationToken));
        }

        if (settings.Validation.WarnInDevelopment && hostEnvironment.IsDevelopment())
        {
            validator.Validate(context);
        }

        return graph;
    }

    public async Task<string?> RenderJsonAsync(IPublishedContent? content = null, CancellationToken cancellationToken = default)
    {
        var graph = await BuildGraphAsync(content, cancellationToken);
        return graph is null ? null : JsonLdSerializer.Serialize(graph, options.Value.WriteIndented);
    }

    public async Task<string> RenderScriptAsync(IPublishedContent? content = null, CancellationToken cancellationToken = default)
        => JsonLdSerializer.ToScriptTag(await RenderJsonAsync(content, cancellationToken));

    private async Task RunProvidersAsync(SchemaContext context, SchemaGraph graph, StructuredDataOptions settings, CancellationToken cancellationToken)
    {
        foreach (var provider in providers.OrderBy(p => p.Order))
        {
            await RunGuardedAsync(graph, provider.GetType().Name, context, async () =>
            {
                if (!provider.AppliesTo(context))
                {
                    return;
                }

                if (!provider.Cacheable || !settings.Cache.Enabled || context.IsPreview)
                {
                    await provider.ContributeAsync(context, cancellationToken);
                    return;
                }

                var nodes = await cache.GetOrCreateAsync(provider, context, settings.Cache.Duration, async () =>
                {
                    var isolated = new SchemaGraph();
                    await provider.ContributeAsync(context.WithGraph(isolated), cancellationToken);
                    return isolated.Nodes;
                });

                foreach (var node in nodes)
                {
                    graph.Add(node);
                }
            });
        }
    }

    private async Task RunElementProvidersAsync(SchemaContext context, SchemaGraph graph, StructuredDataOptions settings, CancellationToken cancellationToken)
    {
        if (settings.BlockProperties.Count == 0 || elementProviders.Count == 0)
        {
            return;
        }

        var ordered = elementProviders.OrderBy(p => p.Order).ToArray();

        foreach (var (block, keys) in BlockWalker.Walk(context.Content, settings.BlockProperties, settings.MaxBlockDepth))
        {
            var matchAll = keys.Contains(StructuredDataOptions.AllProviders);

            foreach (var provider in ordered)
            {
                if (!matchAll && !keys.Contains(provider.Key, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                await RunGuardedAsync(graph, provider.GetType().Name, context, async () =>
                {
                    if (provider.AppliesTo(context, block))
                    {
                        await provider.ContributeAsync(context, block, cancellationToken);
                    }
                });
            }
        }
    }

    /// <summary>
    /// Runs one extension point and records it as the source of the nodes it touches. A failing provider is
    /// logged and skipped, so one broken provider can't break the page the tag is rendered on.
    /// </summary>
    private async Task RunGuardedAsync(SchemaGraph graph, string source, ISchemaContext context, Func<Task> action)
    {
        graph.CurrentSource = source;

        try
        {
            await action();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Structured data {Source} failed for content {ContentId}", source, context.Content.Id);
        }
        finally
        {
            graph.CurrentSource = null;
        }
    }
}
