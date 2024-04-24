using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Chaldea.Fate.RhoAias.Repository;

internal class RhoAiasDbContext : DbContext
{
	public virtual DbSet<Client> Clients { get; set; }
	public virtual DbSet<Proxy> Proxies { get; set; }
	public virtual DbSet<Cert> Certs { get; set; }
	public virtual DbSet<User> Users { get; set; }
	public virtual DbSet<DnsProvider> DnsProviders { get; set; }

	public RhoAiasDbContext(DbContextOptions<RhoAiasDbContext> options)
		: base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder builder)
	{
		builder.Entity<Cert>(b =>
		{
			b.Property(x => x.CertInfo).HasJsonConversion();
		});
		builder.Entity<Cert>()
			.Ignore(t => t.DnsProvider);
	}
}

internal static class ModelBuilderExtensions
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		PropertyNameCaseInsensitive = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	public static ValueConverter<TProvider, string> CreateJsonConverter<TProvider>()
	{
		return new ValueConverter<TProvider, string>(r => r.ToJson(), r => r.FromJson<TProvider>());
	}

	public static PropertyBuilder<TProperty> HasJsonConversion<TProperty>(this PropertyBuilder<TProperty> propertyBuilder)
	{
		return propertyBuilder.HasConversion(CreateJsonConverter<TProperty>());
	}

	public static string ToJson<T>(this T data)
	{
		if (data == null)
		{
			return string.Empty;
		}
		return JsonSerializer.Serialize(data, JsonOptions);
	}

	public static T FromJson<T>(this string data)
	{
		if (string.IsNullOrEmpty(data))
		{
			return default;
		}
		return JsonSerializer.Deserialize<T>(data, JsonOptions);
	}
}