using System.Text.Json.Serialization;

namespace LambdaRegularDemo
{
    [JsonSerializable(typeof(Guid))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    public partial class RegularDemoJsonContext : JsonSerializerContext
    {
    }
}
