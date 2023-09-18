using FileService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileService;

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<CloudFile> Files => Set<CloudFile>();

    public DbSet<AccessToken> AccessTokens => Set<AccessToken>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}