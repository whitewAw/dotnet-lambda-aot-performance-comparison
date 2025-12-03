using Amazon.Lambda.Core;
using LambdaAOTDemo;
using Shared.Services;
using System.Text.Json;

public class Function
{
    private readonly IDynamoDBRepository _dynamoDBRepository;

    public Function(IDynamoDBRepository dynamoDBRepository)
    {
        _dynamoDBRepository = dynamoDBRepository;
    }

    public async Task<Guid> FunctionHandler(Dictionary<string, string> input, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Received input with {input?.Count ?? 0} key-value pairs");

            if (input != null)
            {
                foreach (var kvp in input)
                {
                    context.Logger.LogInformation($"Key: {kvp.Key}, Value: {kvp.Value}");
                }
            }

            using var cts = new CancellationTokenSource(context.RemainingTime);
            var id = await _dynamoDBRepository.CreateAsync(cts.Token);
            return id;
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Exception occurred in Lambda. " +
                                    $"Function: {context.FunctionName};" +
                                    $"RequestId: {context.AwsRequestId};" +
                                    $"Input: {JsonSerializer.Serialize(input, AOTJsonContext.Default.DictionaryStringString)};" +
                                    $"ExceptionType: {ex.GetType().FullName};" +
                                    $"Message: {ex.Message};" +
                                    $"StackTrace: {ex.StackTrace}.");
            throw;
        }
    }
}
