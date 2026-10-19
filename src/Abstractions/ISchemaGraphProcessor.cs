namespace Webwonders.Baseline.StructuredData.Abstractions;

/// <summary>
/// Runs after all providers, block providers and collector contributions, and can change, remove
/// or add anything in the graph before it is rendered.
/// </summary>
public interface ISchemaGraphProcessor
{
    int Order { get; }

    Task ProcessAsync(ISchemaContext context, CancellationToken cancellationToken);
}
