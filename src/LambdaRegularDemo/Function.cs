using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Shared.Services;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace LambdaRegularDemo;

public class Function
{
    private readonly IDynamoDBRepository _dynamoDBRepository;

    public Function(IDynamoDBRepository dynamoDBRepository)
    {
        _dynamoDBRepository = dynamoDBRepository;
    }

    [LambdaFunction]
    public async Task<Guid> FunctionHandler123(Dictionary<string, string> input, ILambdaContext context)
    {
        try
        {
            using var cts = new CancellationTokenSource(context.RemainingTime);
            var id = await _dynamoDBRepository.CreateAsync(cts.Token);
            return id;

        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Exception occurred in Lambda. " +
                                    $"Function: {context.FunctionName};" +
                                    $"RequestId: {context.AwsRequestId};" +
                                    $"Input: {JsonSerializer.Serialize(input, RegularDemoJsonContext.Default.DictionaryStringString)};" +
                                    $"ExceptionType: {ex.GetType().FullName};" +
                                    $"Message: {ex.Message};" +
                                    $"StackTrace: {ex.StackTrace}.");
            throw;
        }
    }
}
