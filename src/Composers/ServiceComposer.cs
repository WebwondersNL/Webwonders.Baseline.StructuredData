using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Webwonders.Baseline.StructuredData.DependencyInjection;

namespace Webwonders.Baseline.StructuredData.Composers;

public class ServiceComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder) => builder.AddStructuredData();
}
