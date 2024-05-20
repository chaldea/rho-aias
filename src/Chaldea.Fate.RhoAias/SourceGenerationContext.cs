using System.Text.Json.Serialization;

namespace Chaldea.Fate.RhoAias;

[JsonSerializable(typeof(Client))]
[JsonSerializable(typeof(Proxy))]
[JsonSerializable(typeof(Result))]
[JsonSerializable(typeof(List<Proxy>))]
[JsonSerializable(typeof(Token))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}