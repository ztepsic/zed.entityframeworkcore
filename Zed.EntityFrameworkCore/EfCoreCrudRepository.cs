using Microsoft.EntityFrameworkCore;
using Zed.Domain;

namespace Zed.EntityFrameworkCore {

    /// <summary>
    /// Entity Framework Core repository with CRUD operations for Entity/Aggregate root with int as identifier type.
    /// </summary>
    /// <typeparam name="TEntity">Entity/Aggregate root type</typeparam>
    public class EfCoreCrudRepository<TEntity> :
        EfCoreCrudRepository<TEntity, int>,
        ICrudRepository<TEntity> where TEntity : Entity {

        /// <summary>
        /// Creates Entity Framework Core CRUD repository
        /// </summary>
        /// <param name="dbContext">DbContext</param>
        public EfCoreCrudRepository(DbContext dbContext) : base(dbContext) { }

    }

    /// <summary>
    /// Entity Framework Core repository with CRUD operations for Entity/Aggregate root
    /// </summary>
    /// <typeparam name="TEntity">Entity/Aggregate root type</typeparam>
    /// <typeparam name="TId">Entity/Aggregate root identifier type</typeparam>
    public class EfCoreCrudRepository<TEntity, TId> :
        EfCoreReadOnlyRepository<TEntity, TId>,
        ICrudRepository<TEntity, TId> where TEntity : Entity<TId> {

        #region Members
        #endregion

        #region Constructors and Init

        /// <summary>
        /// Creates Entity Framework Core CRUD repository
        /// </summary>
        /// <param name="dbContext">DbContext</param>
        public EfCoreCrudRepository(DbContext dbContext) : base(dbContext) { }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all persisted entities/aggregate roots
        /// </summary>
        /// <returns>All persisted entities/aggregate roots</returns>
        public override IEnumerable<TEntity> GetAll() => DbSet.ToList();

        /// <summary>
        /// This is the asynchronous version of <see cref="GetAll()"/>.
        /// Gets all persisted entities/aggregate roots
        /// </summary>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>All persisted entities/aggregate roots</returns>
        public override async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            return await DbSet.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Saves a new or updates an existing entity/aggregate from the repository
        /// </summary>
        /// <param name="entity">Entity/aggregate root which is saved or updated</param>
        public void SaveOrUpdate(TEntity entity) {
            var entry = DbContext.Entry(entity);
            switch (entry.State) {
                case EntityState.Detached:
                    DbContext.Add(entity);
                    break;
                case EntityState.Modified:
                    DbContext.Update(entity);
                    break;
                case EntityState.Added:
                    DbContext.Add(entity);
                    break;
                case EntityState.Unchanged:
                    //item already in db no need to do anything  
                    break;
                default:
                    throw new NotSupportedException($"Unsupported state value: {entry.State}");
            }
        }

        /// <summary>
        /// This is the asynchronous version of <see cref="SaveOrUpdate"/>.
        /// Saves a new or updates an existing entity/aggregate from the repository
        /// </summary>
        /// <param name="entity">Entity/aggregate root which is saved or updated</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        public async Task SaveOrUpdateAsync(TEntity entity, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            var entry = DbContext.Entry(entity);
            switch (entry.State) {
                case EntityState.Detached:
                    await DbContext.AddAsync(entity, cancellationToken).ConfigureAwait(false);
                    break;
                case EntityState.Modified:
                    DbContext.Update(entity);
                    break;
                case EntityState.Added:
                    await DbContext.AddAsync(entity, cancellationToken).ConfigureAwait(false);
                    break;
                case EntityState.Unchanged:
                    //item already in db no need to do anything  
                    break;
                default:
                    throw new NotSupportedException($"Unsupported state value: {entry.State}");
            }
        }

        /// <summary>
        /// Deletes the provided entity/aggregate root from the repository.
        /// </summary>
        /// <param name="entity">Entity/aggregate which needs to be deleted.</param>
        public void Delete(TEntity entity) {
            DbSet.Remove(entity);
        }

        /// <summary>
        /// This is the asynchronous version of <see cref="Delete"/>.
        /// Deletes the provided entity/aggregate root from the repository.
        /// </summary>
        /// <param name="entity">Entity/aggregate which needs to be deleted.</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            DbSet.Remove(entity);
            return Task.CompletedTask;
        }

        #endregion


    }
}
