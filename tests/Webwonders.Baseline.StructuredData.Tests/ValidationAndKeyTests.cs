using Microsoft.Extensions.Logging;
using Schema.NET;
using Webwonders.Baseline.StructuredData.Abstractions;
using Webwonders.Baseline.StructuredData.Validation;

namespace Webwonders.Baseline.StructuredData.Tests;

public class ValidationAndKeyTests
{
    [Fact]
    public void Validator_reports_missing_properties_including_nested_nodes_and_sources()
    {
        var context = TestContent.Context(TestContent.Tree().Home);
        context.Graph.Add(new FAQPage
        {
            Id = context.Ids.ForCurrentPage("faq"),
            MainEntity = new List<IThing> { new Question { Name = "No answer?" } },
        });
        var logger = new ListLogger();

        new SchemaGraphValidator(Array.Empty<IRichResultRule>(), logger).Validate(context);

        var message = Assert.Single(logger.Messages);
        Assert.Contains("AcceptedAnswer", message);
        Assert.DoesNotContain("MainEntity", message);
    }

    [Fact]
    public void Validator_is_quiet_for_complete_nodes_and_skips_reference_only_nodes()
    {
        var context = TestContent.Context(TestContent.Tree().Home);
        var orgId = context.Ids.ForSite("org");
        context.Graph.Add(new LocalBusiness { Id = orgId, Name = "Webwonders", Address = "Klokgebouw 262" });
        context.Graph.Add(new WebPage { Id = context.Ids.ForCurrentPage("webpage"), Publisher = SchemaRef.To<LocalBusiness>(orgId) });
        var logger = new ListLogger();

        new SchemaGraphValidator(Array.Empty<IRichResultRule>(), logger).Validate(context);

        Assert.Empty(logger.Messages);
    }

    [Theory]
    [InlineData(typeof(FaqSchemaProvider), "Faq")]
    [InlineData(typeof(ReviewElementSchemaProvider), "Review")]
    [InlineData(typeof(Products), "Products")]
    public void Element_provider_key_defaults_to_the_class_name_without_suffix(Type type, string expected)
        => Assert.Equal(expected, ElementSchemaProvider<IFake>.DefaultKey(type));

    public interface IFake : Umbraco.Cms.Core.Models.PublishedContent.IPublishedElement;

    private sealed class FaqSchemaProvider;

    private sealed class ReviewElementSchemaProvider;

    private sealed class Products;

    private sealed class ListLogger : ILogger<SchemaGraphValidator>
    {
        public List<string> Messages { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Messages.Add(formatter(state, exception));
    }
}
