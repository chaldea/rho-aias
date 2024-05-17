using System.Text.Json.Serialization;

namespace Chaldea.Fate.RhoAias;

[JsonSerializable(typeof(Client))]
[JsonSerializable(typeof(Proxy))]
[JsonSerializable(typeof(Result))]
[JsonSerializable(typeof(List<Proxy>))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}