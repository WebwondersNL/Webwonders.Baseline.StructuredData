using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Webwonders.Baseline.StructuredData.Abstractions;
using Webwonders.Baseline.StructuredData.Defaults;

namespace Webwonders.Baseline.StructuredData.DependencyInjection;

// Collections are scoped so providers can depend on scoped services.

public sealed class SchemaProviderCollection(Func<IEnumerable<ISchemaProvider>> items)
    : BuilderCollectionBase<ISchemaProvider>(items);

public sealed class SchemaProviderCollectionBuilder
    : OrderedCollectionBuilderBase<SchemaProviderCollectionBuilder, SchemaProviderCollection, ISchemaProvider>
{
    public SchemaProviderCollectionBuilder()
    {
        // Added here rather than in a composer so Remove<T>() works regardless of composer order.
        Append<BreadcrumbListProvider>();
        Append<WebPageProvider>();
    }

    protected override SchemaProviderCollectionBuilder This => this;

    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Scoped;
}

public sealed class ElementSchemaProviderCollection(Func<IEnumerable<IElementSchemaProvider>> items)
    : BuilderCollectionBase<IElementSchemaProvider>(items);

public sealed class ElementSchemaProviderCollectionBuilder
    : OrderedCollectionBuilderBase<ElementSchemaProviderCollectionBuilder, ElementSchemaProviderCollection, IElementSchemaProvider>
{
    protected override ElementSchemaProviderCollectionBuilder This => this;

    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Scoped;
}

public sealed class SchemaGraphProcessorCollection(Func<IEnumerable<ISchemaGraphProcessor>> items)
    : BuilderCollectionBase<ISchemaGraphProcessor>(items);

public sealed class SchemaGraphProcessorCollectionBuilder
    : OrderedCollectionBuilderBase<SchemaGraphProcessorCollectionBuilder, SchemaGraphProcessorCollection, ISchemaGraphProcessor>
{
    protected override SchemaGraphProcessorCollectionBuilder This => this;

    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Scoped;
}
