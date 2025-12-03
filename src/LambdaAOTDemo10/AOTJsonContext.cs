using System.Text.Json.Serialization;

namespace LambdaAOTDemo
{
    [JsonSerializable(typeof(Guid))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    public partial class AOTJsonContext : JsonSerializerContext
    {
    }
}
