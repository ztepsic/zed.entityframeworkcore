using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Zed.EntityFrameworkCore.Test
{
    /// <summary>
    /// Entity Framework Core nUnit fixture
    /// </summary>
    public abstract class EfCoreTestFixture
    {

        #region Fields and Properties

        /// <summary>
        /// DbContext
        /// </summary>
        public DbContext DbContext { get; private set; }

        #endregion

        #region Constructors and Init

        /// <summary>
        /// Creates the DbContext for testing
        /// </summary>
        /// <returns>DbContext instance</returns>
        protected abstract DbContext CreateDbContext();

        #endregion

        #region Methods

        /// <summary>
        /// On fixture setup (async version for compatibility)
        /// </summary>
        public virtual Task OnFixtureSetupAsync()
        {
            OnFixtureSetup();
            return Task.CompletedTask;
        }

        /// <summary>
        /// On fixture tear down (async version for compatibility)
        /// </summary>
        public virtual Task OnFixtureTeardownAsync()
        {
            OnFixtureTeardown();
            return Task.CompletedTask;
        }

        /// <summary>
        /// On fixture setup
        /// </summary>
        protected virtual void OnFixtureSetup() { }

        /// <summary>
        /// On fixture tear down
        /// </summary>
        protected virtual void OnFixtureTeardown() { }

        /// <summary>
        /// On Setup
        /// </summary>
        public virtual void OnSetup() { SetupDbContext(); }

        /// <summary>
        /// On Teardown
        /// </summary>
        public virtual void OnTeardown() { TearDownDbContext(); }

        /// <summary>
        /// Setup DbContext
        /// </summary>
        protected virtual void SetupDbContext()
        {
            DbContext = CreateDbContext();
            BuildSchema();
        }

        /// <summary>
        /// Tear down DbContext
        /// </summary>
        protected virtual void TearDownDbContext()
        {
            DbContext?.Dispose();
            DbContext = null;
        }

        /// <summary>
        /// Builds the database schema
        /// </summary>
        protected virtual void BuildSchema()
        {
            DbContext.Database.EnsureCreated();
        }

        #endregion

    }
}
