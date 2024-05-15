using System.Text.Json.Serialization;

namespace Chaldea.Fate.RhoAias;

[JsonSerializable(typeof(Client))]
[JsonSerializable(typeof(Proxy))]
[JsonSerializable(typeof(Result))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}