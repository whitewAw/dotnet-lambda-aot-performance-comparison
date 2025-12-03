using Amazon.Lambda.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace LambdaReadyToRunDemo
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
