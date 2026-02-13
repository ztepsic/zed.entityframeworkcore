using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zed.EntityFrameworkCore.Test;
using Zed.EntityFrameworkCore.Tests.Model;

namespace Zed.EntityFrameworkCore.Tests.InitDb {
    /// <summary>
    /// Base class for consolidated tests that run against multiple database providers
    /// </summary>
    public abstract class BaseConsolidatedTest {
        private readonly Func<EfCoreTestFixture> _fixtureFactory;
        private EfCoreTestFixture _fixture;

        protected TestDbContext TestDbContext => (TestDbContext)_fixture.DbContext;

        protected BaseConsolidatedTest(Func<EfCoreTestFixture> fixtureFactory) {
            _fixtureFactory = fixtureFactory;
        }

        protected static IEnumerable<TestFixtureData> GetFixtureSources() {
            yield return new TestFixtureData(new Func<EfCoreTestFixture>(() => new SqliteEfCoreTestFixture()))
                .SetArgDisplayNames("SQLite");
            yield return new TestFixtureData(new Func<EfCoreTestFixture>(() => new PostgresEfCoreTestFixture()))
                .SetArgDisplayNames("Postgres");
        }

        [OneTimeSetUp]
        public async Task FixtureSetup() {
            _fixture = _fixtureFactory();
            await _fixture.OnFixtureSetupAsync();
        }

        [OneTimeTearDown]
        public async Task FixtureTearDown() {
            if (_fixture != null) {
                await _fixture.OnFixtureTeardownAsync();
            }
        }

        [SetUp]
        public void Setup() {
            _fixture.OnSetup();
            OnTestSetup();
        }

        [TearDown]
        public void TearDown() {
            OnTestTearDown();
            _fixture.OnTeardown();
        }

        /// <summary>
        /// Override this method to perform test-specific setup
        /// </summary>
        protected virtual void OnTestSetup() {
        }

        /// <summary>
        /// Override this method to perform test-specific teardown
        /// </summary>
        protected virtual void OnTestTearDown() {
        }
    }
}
