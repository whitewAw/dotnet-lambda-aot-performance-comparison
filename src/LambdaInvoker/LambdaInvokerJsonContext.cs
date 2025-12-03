using System.Text.Json.Serialization;

namespace LambdaInvoker
{
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(List<string>))]
    public partial class LambdaInvokerJsonContext : JsonSerializerContext
    {
    }
}
