using Amazon.Lambda;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using LambdaInvoker;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<LambdaInvokerJsonContext>))]
namespace LambdaInvoker
{
    public class Startup
    {
        private static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddSingleton<IAmazonLambda>(sp => new AmazonLambdaClient());

            var provider = services.BuildServiceProvider();

            using var scope = provider.CreateScope();
            {
                var iAmazonLambda = scope.ServiceProvider.GetRequiredService<IAmazonLambda>();
                var function = new Function(iAmazonLambda);

                Func<List<string>, ILambdaContext, Task> handler = (input, context) =>
                {
                    function.FunctionHandler(input, context);
                    return Task.CompletedTask;
                };

                await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<LambdaInvokerJsonContext>())
                                            .Build()
                                            .RunAsync();
            }
        }
    }
}
