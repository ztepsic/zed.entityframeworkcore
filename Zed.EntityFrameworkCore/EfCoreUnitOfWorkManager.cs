using Microsoft.EntityFrameworkCore;
using Zed.Transaction;

namespace Zed.EntityFrameworkCore {
    /// <summary>
    /// Entity Framework Core Unit of Work manager
    /// </summary>
    /// <remarks>Based on article: http://www.planetgeek.ch/2012/05/05/what-is-that-all-about-the-repository-anti-pattern/ </remarks>
    public class EfCoreUnitOfWorkManager : IUnitOfWorkManager {

        #region Fields and Properties

        private readonly DbContext dbContext;
        private readonly Func<IUnitOfWork> rootScopeFactory;
        private readonly Func<IUnitOfWork> dependentScopeFactory;

        /// <summary>
        /// Tracks whether root scope has been created for this unit of work instance
        /// </summary>
        private bool isRootScopeCreated;

        /// <inheritdoc/>
        public bool IsImplicitTransactionsEnabled { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates Entity Framework Core Unit of Work
        /// </summary>
        /// <param name="dbContext">Entity Framework Core DbContext</param>
        /// <param name="rootScopeFactory">Root transaction scope factory</param>
        /// <param name="dependentScopeFactory">Dependent transaction scope factory</param>
        /// <param name="isImplicitTransactionsEnabled">When true, automatically starts a new transaction after commit/rollback</param>
        public EfCoreUnitOfWorkManager(
            DbContext dbContext,
            Func<IUnitOfWork> rootScopeFactory,
            Func<IUnitOfWork> dependentScopeFactory,
            bool isImplicitTransactionsEnabled = false) {

            this.dbContext = dbContext;
            this.rootScopeFactory = rootScopeFactory;
            this.dependentScopeFactory = dependentScopeFactory;
            IsImplicitTransactionsEnabled = isImplicitTransactionsEnabled;

        }

        /// <summary>
        /// Creates Entity Framework Core Unit of Work with implicit transaction support.
        /// Relies on EF Core's implicit transaction during SaveChanges (no explicit BeginTransaction call).
        /// Automatically restarts transactions after commit/rollback.
        /// </summary>
        /// <param name="dbContext">Entity Framework Core DbContext</param>
        /// <returns>A new instance of <see cref="EfCoreUnitOfWorkManager"/> with implicit transaction mode and automatic restart enabled</returns>
        public static EfCoreUnitOfWorkManager Create(DbContext dbContext)
            => new(
                dbContext,
                () => EfCoreUnitOfWork.CreateRootScope(dbContext),
                () => EfCoreUnitOfWork.CreateDependedScope(dbContext),
                true);

        /// <summary>
        /// Creates Entity Framework Core Unit of Work with explicit transaction support.
        /// Explicitly calls BeginTransaction on the DbContext.
        /// </summary>
        /// <param name="dbContext">Entity Framework Core DbContext</param>
        /// <param name="isImplicitTransactionsEnabled">When true, automatically starts a new transaction after commit/rollback. When false, transactions must be manually started.</param>
        /// <returns>A new instance of <see cref="EfCoreUnitOfWorkManager"/> with explicit transaction control</returns>
        public static EfCoreUnitOfWorkManager CreateWithExplicitTransaction(DbContext dbContext, bool isImplicitTransactionsEnabled = false)
             => new(
                 dbContext,
                 () => EfCoreUnitOfWork.CreateRootScopeWithExplicitTransaction(dbContext, isImplicitTransactionsEnabled),
                 () => EfCoreUnitOfWork.CreateDependedScopeWithExplicitTransaction(dbContext, isImplicitTransactionsEnabled),
                 isImplicitTransactionsEnabled);

        #endregion

        #region Methods

        /// <inheritdoc/>
        public IUnitOfWork Start() {
            var scope = !isRootScopeCreated
                ? rootScopeFactory()
                : dependentScopeFactory();

            if (!isRootScopeCreated) {
                isRootScopeCreated = true;
            }

            scope.BeginTransaction();

            return scope;
        }

        /// <inheritdoc/>
        public async Task<IUnitOfWork> StartAsync()
            => await StartAsync(CancellationToken.None).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<IUnitOfWork> StartAsync(CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            var scope = !isRootScopeCreated
               ? rootScopeFactory()
               : dependentScopeFactory();

            if (!isRootScopeCreated) {
                isRootScopeCreated = true;
            }


            await scope.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            return scope;
        }

        #endregion

    }
}
