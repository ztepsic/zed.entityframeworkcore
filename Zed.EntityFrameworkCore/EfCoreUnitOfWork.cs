using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Zed.Transaction;

namespace Zed.EntityFrameworkCore {
    /// <summary>
    /// Entity Framework Core Unit Of Work
    /// </summary>
    /// <remarks>
    /// <para>Based on article: http://www.planetgeek.ch/2012/05/05/what-is-that-all-about-the-repository-anti-pattern/</para>
    /// <para>Transaction article: https://learn.microsoft.com/en-us/ef/core/saving/transactions</para>
    /// <para>
    /// <b>Transaction Management Modes:</b>
    /// <list type="bullet">
    /// <item>
    /// <term>useExplicitTransaction</term>
    /// <description>Controls HOW transactions are started. When true, explicitly calls DbContext.Database.BeginTransaction(). 
    /// When false, relies on EF Core's implicit transaction created during SaveChanges().</description>
    /// </item>
    /// <item>
    /// <term>isImplicitTransactionsEnabled</term>
    /// <description>Controls WHEN the next transaction starts. When true, automatically starts a new transaction after commit/rollback. 
    /// When false, requires manual transaction start for each operation.</description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    public class EfCoreUnitOfWork : IUnitOfWork, IAsyncDisposable {

        #region Fields and Properties

        /// <summary>
        /// Entity Framework Core DbContext
        /// </summary>
        private readonly DbContext dbContext;

        /// <summary>
        /// Gets Entity Framework Core DbContext
        /// </summary>
        protected DbContext DbContext => dbContext;

        /// <summary>
        /// Gets current transaction
        /// </summary>
        protected IDbContextTransaction? Transaction => dbContext.Database.CurrentTransaction;

        /// <summary>
        /// Indicates if transaction is created
        /// </summary>
        private bool isTransactionCreated;

        /// <summary>
        /// Indicates if transaction scope is completed
        /// </summary>
        private bool isScopeCompleted;

        /// <summary>
        /// An indicator if transaction is active or not
        /// </summary>
        /// <returns></returns>
        public bool IsTransactionActive => Transaction != null;

        /// <summary>
        /// An indication if this scope is root or dependent/nested scope
        /// </summary>
        private readonly bool isRootScope;

        /// <summary>
        /// Indicates whether explicit transaction management is used.
        /// When true, DbContext.Database.BeginTransaction() is explicitly called.
        /// When false, relies on EF Core's implicit transaction during SaveChanges.
        /// </summary>
        private readonly bool useExplicitTransaction;

        /// <summary>
        /// Indicates whether automatic transaction restart is enabled.
        /// When true, a new transaction is automatically started after commit or rollback.
        /// When false, transactions must be manually started after each commit/rollback.
        /// </summary>
        private readonly bool isImplicitTransactionsEnabled;

        /// <summary>
        /// Gets an indication whether automatic transaction restart is enabled.
        /// When true, a new transaction is automatically started after commit or rollback.
        /// </summary>
        public bool IsImplicitTransactionsEnabled => isImplicitTransactionsEnabled;

        #endregion

        #region Constructors and Init

        /// <summary>
        /// Creates Entity Framework Core unit of work scope
        /// </summary>
        /// <param name="dbContext">Entity Framework Core DbContext</param>
        /// <param name="isImplicitTransactionsEnabled">When true, automatically starts a new transaction after commit/rollback. When false, transactions must be manually started.</param>
        /// <param name="isRootScope">An indication if this is a root scope or a dependent/nested scope</param>
        /// <param name="useExplicitTransaction">When true, explicitly calls BeginTransaction on DbContext. When false, relies on EF Core's implicit transaction during SaveChanges.</param>
        protected EfCoreUnitOfWork(
            DbContext dbContext,
            bool isImplicitTransactionsEnabled = true,
            bool isRootScope = true,
            bool useExplicitTransaction = false) {
            this.dbContext = dbContext;
            this.isImplicitTransactionsEnabled = isImplicitTransactionsEnabled;
            this.isRootScope = isRootScope;
            this.useExplicitTransaction = useExplicitTransaction;
        }

        /// <summary>
        /// Creates a root scope with implicit transaction support.
        /// This is the outermost scope that will manage the transaction lifecycle.
        /// </summary>
        /// <param name="dbContext">Entity Framework Core DbContext</param>
        /// <returns>A new root scope instance</returns>
        public static EfCoreUnitOfWork CreateRootScope(DbContext dbContext)
         => new(dbContext, isImplicitTransactionsEnabled: true, isRootScope: true);

        /// <summary>
        /// Creates a root scope with explicit transaction management.
        /// This scope will explicitly call BeginTransaction on the DbContext.
        /// </summary>
        /// <param name="dbContext">Entity Framework Core DbContext</param>
        /// <param name="isImplicitTransactionsEnabled">When true, automatically starts a new transaction after commit/rollback</param>
        /// <returns>A new root scope instance with explicit transaction control</returns>
        public static EfCoreUnitOfWork CreateRootScopeWithExplicitTransaction(DbContext dbContext, bool isImplicitTransactionsEnabled = false)
             => new(dbContext, isImplicitTransactionsEnabled: isImplicitTransactionsEnabled, isRootScope: true, useExplicitTransaction: true);

        /// <summary>
        /// Creates a dependent/nested scope with implicit transaction support.
        /// This scope participates in an existing transaction created by a root scope.
        /// </summary>
        /// <param name="dbContext">Entity Framework Core DbContext</param>
        /// <returns>A new dependent scope instance</returns>
        public static EfCoreUnitOfWork CreateDependedScope(DbContext dbContext)
            => new(dbContext, isImplicitTransactionsEnabled: true, isRootScope: false);

        /// <summary>
        /// Creates a dependent/nested scope with explicit transaction management.
        /// This scope participates in an existing explicit transaction created by a root scope.
        /// </summary>
        /// <param name="dbContext">Entity Framework Core DbContext</param>
        /// <param name="isImplicitTransactionsEnabled">When true, automatically starts a new transaction after commit/rollback</param>
        /// <returns>A new dependent scope instance with explicit transaction control</returns>
        public static EfCoreUnitOfWork CreateDependedScopeWithExplicitTransaction(DbContext dbContext, bool isImplicitTransactionsEnabled = false)
            => new(dbContext, isImplicitTransactionsEnabled: isImplicitTransactionsEnabled, isRootScope: false, useExplicitTransaction: true);

        #endregion

        #region Methods

        /// <summary>
        /// Begins/starts with transaction
        /// </summary>
        public virtual void BeginTransaction() {
            if (useExplicitTransaction) {
                if (!IsTransactionActive) {
                    isTransactionCreated = true;
                    _ = DbContext.Database.BeginTransaction();
                }
            } else {
                if (isRootScope) {
                    isTransactionCreated = true;
                }
            }
        }

        /// <summary>
        /// This is the asynchronous version of <see cref="BeginTransaction"/>.
        /// This method invokes the virtual method <see cref="BeginTransactionAsync()"/> with CancellationToken.None.
        /// Begins/starts with transaction
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task BeginTransactionAsync()
            => await BeginTransactionAsync(CancellationToken.None).ConfigureAwait(false);

        /// <summary>
        /// This is the asynchronous version of <see cref="BeginTransaction"/>.
        /// Begins/starts with transaction
        /// </summary>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();

            if (useExplicitTransaction) {
                if (!IsTransactionActive) {
                    isTransactionCreated = true;
                    _ = await DbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                }
            } else {
                if (isRootScope) {
                    isTransactionCreated = true;
                }
            }
        }

        /// <summary>
        /// Commits transaction
        /// </summary>
        public virtual void Commit() {
            isScopeCompleted = true;
            if (isTransactionCreated) {
                DbContext.SaveChanges();

                Transaction?.Commit();

                if (isImplicitTransactionsEnabled) {
                    isScopeCompleted = false;
                    BeginTransaction();
                }
            }
        }

        /// <summary>
        /// Commits transaction
        /// </summary>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task CommitAsync(CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();

            isScopeCompleted = true;
            if (isTransactionCreated) {
                await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (Transaction != null) {
                    await Transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                }

                if (isImplicitTransactionsEnabled) {
                    isScopeCompleted = false;
                    await BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Commits transaction
        /// This method invokes the virtual method <see cref="CommitAsync()"/> with CancellationToken.None.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task CommitAsync()
            => await CommitAsync(CancellationToken.None).ConfigureAwait(false);

        /// <summary>
        /// Reverts all tracked entity changes in the DbContext ChangeTracker.
        /// This ensures the in-memory context state matches the database state after transaction rollback.
        /// </summary>
        /// <remarks>
        /// Handles the following entity states:
        /// <list type="bullet">
        /// <item><description>Modified: Reverts to original values</description></item>
        /// <item><description>Added: Detaches from context</description></item>
        /// <item><description>Deleted: Restores to unchanged state</description></item>
        /// </list>
        /// </remarks>
        private void RollbackDbContext() {
            foreach (var entry in dbContext.ChangeTracker.Entries()) {
                switch (entry.State) {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;

                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Detached:
                        // No action needed - already in correct state
                        break;
                }
            }

        }

        /// <summary>
        /// Rollbacks transaction
        /// </summary>
        public virtual void Rollback() {
            isScopeCompleted = true;
            if (isTransactionCreated) {
                RollbackDbContext();
                Transaction?.Rollback();

                if (isImplicitTransactionsEnabled) {
                    isScopeCompleted = false;
                    BeginTransaction();
                }
            }
        }

        /// <summary>
        /// Rollbacks transaction
        /// </summary>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task RollbackAsync(CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();

            isScopeCompleted = true;
            if (isTransactionCreated) {

                RollbackDbContext();

                if (Transaction != null) {
                    await Transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                }

                if (isImplicitTransactionsEnabled) {
                    isScopeCompleted = false;
                    await BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Rollbacks transaction
        /// This method invokes the virtual method <see cref="RollbackAsync()"/> with CancellationToken.None.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task RollbackAsync()
            => await RollbackAsync(CancellationToken.None).ConfigureAwait(false);

        /// <inheritdoc/>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {

                if (!isScopeCompleted && Transaction != null) {
                    Rollback();
                }

                if (isTransactionCreated && Transaction != null) {
                    Transaction?.Dispose();
                }
            }
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync() {
            await DisposeAsync(true).ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs async application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">A value indicating whether the method is being called from Dispose or from a finalizer.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual async ValueTask DisposeAsync(bool disposing) {
            if (disposing) {

                if (!isScopeCompleted && Transaction != null) {
                    await RollbackAsync(CancellationToken.None).ConfigureAwait(false);
                }

                if (isTransactionCreated && Transaction != null) {
                    await Transaction.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion

    }
}
