using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TechXyz.GymXyz.Application.Interfaces;
using TechXyz.GymXyz.Persistence.Contexts;

namespace TechXYZ.GymXYZ.Application.Tests.Members;

internal static class RelationalTestInfrastructure
{
    public static async Task<SqliteDbContextScope> CreateSqliteDbContextScope()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<GymDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new GymDbContext(options, new TestCurrentUserService());
        await dbContext.Database.EnsureCreatedAsync();

        return new SqliteDbContextScope(dbContext, connection);
    }

    internal sealed class SqliteDbContextScope : IAsyncDisposable
    {
        public SqliteDbContextScope(GymDbContext dbContext, SqliteConnection connection)
        {
            DbContext = dbContext;
            Connection = connection;
        }

        public GymDbContext DbContext { get; }
        private SqliteConnection Connection { get; }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public string? UserName => "test-user";
    }
}
