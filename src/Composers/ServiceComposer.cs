using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Webwonders.Baseline.StructuredData.Interfaces;
using Webwonders.Baseline.StructuredData.Options;
using Webwonders.Baseline.StructuredData.Services;

namespace Webwonders.Baseline.StructuredData.Composers;

public class ServiceComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddOptions<StructuredDataSettings>()
            .Bind(builder.Config.GetSection(StructuredDataSettings.ConfigurationName));

        builder.Services.AddTransient<IStructuredDataService, StructuredDataService>();
    }
}