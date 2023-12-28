using Microsoft.EntityFrameworkCore;
using Post.Query.Domain.Entities;

namespace Post.Query.Infrastructure.DataAccess;

public class DatabaseContextFactory
{
    private readonly Action<DbContextOptionsBuilder> _configureDbContext;

    public DatabaseContextFactory(Action<DbContextOptionsBuilder> configureDbContext) =>
     this._configureDbContext = configureDbContext;

     public DatabaseContext CreateDbContext() {
        DbContextOptionsBuilder<DatabaseContext> optionsBuilder = new();
        this._configureDbContext(optionsBuilder);
        return new DatabaseContext(optionsBuilder.Options);
     }
}