using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Zed.EntityFrameworkCore.Test;
using Zed.EntityFrameworkCore.Tests.Model;

namespace Zed.EntityFrameworkCore.Tests.InitDb {
    [TestFixture]
    public class PostgresEfCoreTestFixture : EfCoreTestFixture {

#pragma warning disable NUnit1032 // Suppress warning about field disposal - _container is properly disposed in OnFixtureTeardownAsync
        private PostgreSqlContainer _container;
#pragma warning restore NUnit1032
        protected TestDbContext TestDbContext { get { return (TestDbContext)DbContext; } }

        protected override DbContext CreateDbContext() {
            var contextOptions = new DbContextOptionsBuilder<TestDbContext>()
                    .UseNpgsql(_container.GetConnectionString())
                    .Options;
            var dbContext = new TestDbContext(contextOptions);

            return dbContext;
        }

        public override async Task OnFixtureSetupAsync() {
            _container = new PostgreSqlBuilder("postgres:15")
                .Build();

            await _container.StartAsync();
            OnFixtureSetup();
        }

        public override async Task OnFixtureTeardownAsync() {
            OnFixtureTeardown();
            if (_container != null) {
                await _container.StopAsync();
                await _container.DisposeAsync();
            }
        }

        [SetUp]
        public void Setup() { OnSetup(); }

        public override void OnTeardown() {
            // Close connections
            if (DbContext != null) {
                TestDbContext.Database.CloseConnection();
                // Clean up by dropping and recreating the public schema
                // This is faster than EnsureDeleted() and avoids the "cannot drop database" error
                TestDbContext.Database.ExecuteSql($"DROP SCHEMA IF EXISTS public CASCADE;");
                TestDbContext.Database.ExecuteSql($"CREATE SCHEMA public;");
            }
            // Dispose the context
            base.OnTeardown();
        }

        [TearDown]
        public void TearDown() {
            OnTeardown();
        }

    }
}
