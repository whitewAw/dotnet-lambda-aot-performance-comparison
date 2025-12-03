using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using LambdaAOTDemo;
using Microsoft.Extensions.DependencyInjection;
using Shared;
using Shared.Services;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<AOTJsonContext>))]
namespace LambdaAOTDemo
{
    public class Startup
    {
        private static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            SharedStartup.ConfigureSharedServices(services);

            var provider = services.BuildServiceProvider();

            using var scope = provider.CreateScope();
            {
                var iRepository = scope.ServiceProvider.GetRequiredService<IDynamoDBRepository>();
                var function = new Function(iRepository);

                Func<Dictionary<string, string>, ILambdaContext, Task<Guid>> handler = function.FunctionHandler;

                await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<AOTJsonContext>())
                                            .Build()
                                            .RunAsync();
            }
        }
    }
}


