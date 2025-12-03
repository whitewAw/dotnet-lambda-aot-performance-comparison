using Amazon.Lambda;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;

namespace LambdaInvoker
{
    public class Function
    {
        private readonly IAmazonLambda _lambdaClient;

        public Function(IAmazonLambda lambdaClient)
        {
            _lambdaClient = lambdaClient ?? throw new ArgumentNullException(nameof(lambdaClient));
        }

        public void FunctionHandler(List<string> targetFunctionArns, ILambdaContext context)
        {
            if (targetFunctionArns == null || targetFunctionArns.Count == 0)
            {
                context.Logger.LogError("No target function ARNs provided");
                throw new ArgumentException("Target function ARNs list cannot be null or empty", nameof(targetFunctionArns));
            }

            context.Logger.LogInformation("Starting tests for {Count} Lambda functions", targetFunctionArns.Count);

            foreach (var targetFunctionArn in targetFunctionArns)
            {
                // Extract function name from ARN (format: arn:aws:lambda:region:account:function:function-name)
                var functionName = targetFunctionArn.Split(':').LastOrDefault() ?? targetFunctionArn;

                // Get function configuration to retrieve package size
                var getFunctionRequest = new GetFunctionRequest
                {
                    FunctionName = targetFunctionArn
                };

                var getFunctionResponse = _lambdaClient.GetFunctionAsync(getFunctionRequest).GetAwaiter().GetResult();
                var packageSize = getFunctionResponse.Configuration.CodeSize;
                var packageSizeMB = packageSize / (1024.0 * 1024.0);

                context.Logger.LogInformation("Testing Function: {FunctionName} | Package Size: {PackageSizeMB:F2} MB", functionName, packageSizeMB);

                RunTestsForFunction(targetFunctionArn, context);
            }
        }

        private void RunTestsForFunction(string targetFunctionArn, ILambdaContext context)
        {
            try
            {
                using var cts = new CancellationTokenSource(context.RemainingTime);
                long maxMemoryUsed = 0;
                long billedDuration = 0;

                // Cold start - single invocation
                {
                    //context.Logger.LogInformation("=== COLD START TEST ===");
                    //context.Logger.LogInformation("Invoking Lambda function: {targetFunctionArn}", targetFunctionArn);

                    var payloadJson = "{\"key1\": \"value1\"}";
                    var invokeRequest = new InvokeRequest
                    {
                        FunctionName = targetFunctionArn,
                        InvocationType = "RequestResponse",
                        Payload = payloadJson,
                        LogType = "Tail" // Request log tail to get memory usage
                    };

                    var invokeResponse = _lambdaClient.InvokeAsync(invokeRequest, cts.Token).GetAwaiter().GetResult();

                    // Extract max memory used and billed duration from log result
                    if (!string.IsNullOrEmpty(invokeResponse.LogResult))
                    {
                        var logBytes = Convert.FromBase64String(invokeResponse.LogResult);
                        var logText = System.Text.Encoding.UTF8.GetString(logBytes);
                        maxMemoryUsed = ExtractMaxMemoryUsed(logText);
                        billedDuration = ExtractBilledDuration(logText);
                    }

                    context.Logger.LogInformation("COLD RUN - Billed Duration: {BilledDuration}ms | Memory Used: {MaxMemory} MB",
                        billedDuration, maxMemoryUsed);

                    if (invokeResponse.FunctionError != null)
                    {
                        context.Logger.LogError("Lambda function returned an error: {Error}", invokeResponse.FunctionError);
                    }
                }

                // Warm runs
                const int warmRunCount = 100;
                long totalWarmDuration = 0;
                var warmRunDurations = new List<long>();
                maxMemoryUsed = 0;

                for (int i = 1; i <= warmRunCount; i++)
                {
                    var payloadJson = "{\"key1\": \"value1\"}";
                    var invokeRequest = new InvokeRequest
                    {
                        FunctionName = targetFunctionArn,
                        InvocationType = "RequestResponse",
                        Payload = payloadJson,
                        LogType = "Tail" // Get logs for every invocation to track billed duration
                    };

                    var invokeResponse = _lambdaClient.InvokeAsync(invokeRequest, cts.Token).GetAwaiter().GetResult();

                    // Extract billed duration and memory used from each run
                    long currentBilledDuration = 0;
                    if (!string.IsNullOrEmpty(invokeResponse.LogResult))
                    {
                        var logBytes = Convert.FromBase64String(invokeResponse.LogResult);
                        var logText = System.Text.Encoding.UTF8.GetString(logBytes);
                        var memoryUsed = ExtractMaxMemoryUsed(logText);
                        currentBilledDuration = ExtractBilledDuration(logText);

                        if (memoryUsed > maxMemoryUsed)
                        {
                            maxMemoryUsed = memoryUsed;
                        }
                    }

                    warmRunDurations.Add(currentBilledDuration);
                    totalWarmDuration += currentBilledDuration;

                    if (invokeResponse.FunctionError != null)
                    {
                        context.Logger.LogError("Lambda function returned an error on warm run #{RunNumber}: {Error}", i, invokeResponse.FunctionError);
                    }
                }

                // Calculate and log final statistics
                var averageDuration = totalWarmDuration / (double)warmRunCount;
                var minDuration = warmRunDurations.Min();
                var maxDuration = warmRunDurations.Max();

                //context.Logger.LogInformation("=== WARM RUN STATISTICS ===");
                context.Logger.LogInformation("WARM RUN - Total Runs: {TotalRuns} | Total Billed Duration: {TotalDuration}ms | Average Duration: {AvgDuration:F2}ms | Min Duration: {MinDuration}ms | Max Duration: {MaxDuration}ms | Memory Used: {MaxMemory} MB",
                    warmRunCount, totalWarmDuration, averageDuration, minDuration, maxDuration, maxMemoryUsed);
            }
            catch (Exception ex)
            {
                context.Logger.LogError(ex, "Error invoking Lambda function: {FunctionArn}", targetFunctionArn);
                throw;
            }
        }

        private long ExtractMaxMemoryUsed(string logText)
        {
            // Look for pattern: "Max Memory Used: X MB" in CloudWatch logs
            var match = System.Text.RegularExpressions.Regex.Match(logText, @"Max Memory Used:\s*(\d+)\s*MB");
            if (match.Success && long.TryParse(match.Groups[1].Value, out var memoryUsed))
            {
                return memoryUsed;
            }
            return 0;
        }

        private long ExtractBilledDuration(string logText)
        {
            // Look for pattern: "Billed Duration: X ms" in CloudWatch logs
            var match = System.Text.RegularExpressions.Regex.Match(logText, @"Billed Duration:\s*(\d+)\s*ms");
            if (match.Success && long.TryParse(match.Groups[1].Value, out var billedDuration))
            {
                return billedDuration;
            }
            return 0;
        }
    }
}
