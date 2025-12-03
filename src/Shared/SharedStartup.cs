using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Options;
using Shared.Services;


namespace Shared
{
    public static class SharedStartup
    {
        public static void ConfigureSharedServices(IServiceCollection services)
        {
            services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            });

            services.AddAWSService<IAmazonDynamoDB>();
            services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
            services.AddOptions<DynamoDbOptions>()
               .Configure(options =>
               {
                   var ttlDaysEnv = Environment.GetEnvironmentVariable("DYNAMODB_TTL_DAYS");
                   if (!string.IsNullOrWhiteSpace(ttlDaysEnv) && int.TryParse(ttlDaysEnv, out var ttl))
                   {
                       options.TTLDays = ttl;
                   }
                   var fullTableName = Environment.GetEnvironmentVariable("DYNAMODB_TABLE_NAME");
                   if (!string.IsNullOrWhiteSpace(fullTableName))
                   {
                       options.FullTableName = fullTableName;
                   }
               })
               .Validate(options => !string.IsNullOrWhiteSpace(options.FullTableName), "DYNAMODB_TABLE_NAME must not be null or empty"); ;

            services.AddScoped<IDynamoDBRepository, DynamoDbRepository>();
        }
    }
}
