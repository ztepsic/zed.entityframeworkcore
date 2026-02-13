using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Data.Common;
using Zed.EntityFrameworkCore.Test;
using Zed.EntityFrameworkCore.Tests.Model;

namespace Zed.EntityFrameworkCore.Tests.InitDb {
    [TestFixture]
    public class SqliteEfCoreTestFixture : EfCoreTestFixture {

        private const string CONNECTION_STRING = "Filename=:memory:;Foreign Keys=false;";

        protected DbConnection Connection { get; private set; }
        protected TestDbContext TestDbContext { get { return (TestDbContext)DbContext; } }

        protected override DbContext CreateDbContext() {
            Connection = new SqliteConnection(CONNECTION_STRING);
            Connection.Open();
            var contextOptions = new DbContextOptionsBuilder<TestDbContext>()
                    .UseSqlite(Connection)
                    .Options;
            var dbContext = new TestDbContext(contextOptions);

            return dbContext;
        }

        [OneTimeSetUp]
        public void FixtureSetup() { OnFixtureSetup(); }

        [OneTimeTearDown]
        public void FixtureTearDown() { OnFixtureTeardown(); }

        [SetUp]
        public void Setup() { OnSetup(); }

        public override void OnTeardown() {
            if (Connection != null) {
                Connection.Close();
                DbContext?.Database.EnsureDeleted();
            }
            base.OnTeardown();
        }

        [TearDown]
        public void TearDown() {
            OnTeardown();
        }

    }
}
