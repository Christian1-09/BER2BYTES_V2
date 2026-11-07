using Microsoft.EntityFrameworkCore;

namespace Ber2Bytes.Infrastructure.Persistence;

/// <summary>Wraps EF Core's context factory behind the Application's IAppDbContextFactory.</summary>
public class AppDbContextFactory(IDbContextFactory<AppDbContext> inner) : IAppDbContextFactory
{
    public IAppDbContext Create() => inner.CreateDbContext();
    public async Task<IAppDbContext> CreateAsync() => await inner.CreateDbContextAsync();
}
