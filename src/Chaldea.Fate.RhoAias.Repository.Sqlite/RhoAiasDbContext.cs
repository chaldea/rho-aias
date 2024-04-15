using Microsoft.EntityFrameworkCore;

namespace Chaldea.Fate.RhoAias.Repository;

internal class RhoAiasDbContext : DbContext
{
	public virtual DbSet<Client> Clients { get; set; }
	public virtual DbSet<Proxy> Proxies { get; set; }

	public RhoAiasDbContext(DbContextOptions<RhoAiasDbContext> options)
		: base(options)
	{

	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
	}
}