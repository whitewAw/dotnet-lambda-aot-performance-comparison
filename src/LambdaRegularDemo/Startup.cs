using Amazon.Lambda.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace LambdaRegularDemo
{
    [LambdaStartup]
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            SharedStartup.ConfigureSharedServices(services);
        }
    }
}
