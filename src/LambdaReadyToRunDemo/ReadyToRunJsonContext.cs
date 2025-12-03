using System.Text.Json.Serialization;

namespace LambdaReadyToRunDemo;

[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class ReadyToRunJsonContext : JsonSerializerContext
{
}
