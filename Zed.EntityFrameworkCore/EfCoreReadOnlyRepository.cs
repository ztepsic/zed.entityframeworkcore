using Microsoft.EntityFrameworkCore;
using Zed.Domain;

namespace Zed.EntityFrameworkCore {

    /// <summary>
    /// Entity Framework Core read-only repository for Entity/Aggregate root with int as identifier type.
    /// </summary>
    /// <typeparam name="TEntity">Entity/Aggregate root type</typeparam>
    public class EfCoreReadOnlyRepository<TEntity> :
        EfCoreReadOnlyRepository<TEntity, int>,
        IReadOnlyRepository<TEntity> where TEntity : Entity {

        /// <summary>
        /// Creates Entity Framework Core read-only repository
        /// </summary>
        /// <param name="dbContext">DbContext</param>
        public EfCoreReadOnlyRepository(DbContext dbContext) : base(dbContext) { }
    }

    /// <summary>
    /// Entity Framework Core read-only repository
    /// </summary>
    /// <typeparam name="TEntity">Entity/Aggregate root type</typeparam>
    /// <typeparam name="TId">Entity/Aggregate root identifier type</typeparam>
    public class EfCoreReadOnlyRepository<TEntity, TId> :
        EfCoreRepository, IReadOnlyRepository<TEntity, TId> where TEntity : Entity<TId> {

        #region Fields and Properties

        /// <summary>
        /// Represents the Entity Framework set for the entity type managed by this repository.
        /// </summary>
        /// <remarks>This field provides access to the underlying <see cref="DbSet{TEntity}"/> used for
        /// querying and saving instances of <typeparamref name="TEntity"/>. It is intended for use by derived
        /// repository classes to perform data operations.</remarks>
        protected readonly DbSet<TEntity> DbSet;

        #endregion

        #region Constructors and Init

        /// <summary>
        /// Creates Entity Framework Core read-only repository
        /// </summary>
        /// <param name="dbContext">DbContext</param>
        public EfCoreReadOnlyRepository(DbContext dbContext) : base(dbContext) {
            DbSet = DbContext.Set<TEntity>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all persisted entities/aggregate roots
        /// </summary>
        /// <returns>All persisted entities/aggregate roots</returns>
        public virtual IEnumerable<TEntity> GetAll()
            => DbSet.AsNoTracking().ToList();

        /// <summary>
        /// This is the asynchronous version of <see cref="GetAll()"/>.
        /// Gets all persisted entities/aggregate roots
        /// </summary>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>All persisted entities/aggregate roots</returns>
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            return await DbSet.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets entity/aggregate root bases on it's identity.
        /// </summary>
        /// <param name="id">Entity/Aggregat root identifier</param>
        /// <returns>Entity/aggregate root</returns>
        public virtual TEntity? GetById(TId id) => DbSet.Find(id);

        /// <summary>
        /// This is the asynchronous version of <see cref="GetById"/>.
        /// Gets entity/aggregate root bases on it's identity.
        /// </summary>
        /// <param name="id">Entity/Aggregat root identifier</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>Entity/aggregate root</returns>
        public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken) {
            return await DbSet.FindAsync([id], cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Return the persistent instance of the given entity class with the given identifier,
        /// assuming that the instnace exists.
        /// </summary>
        /// <param name="id">A valid identifier of an existing persistent instance of the class</param>
        /// <returns>Entity/aggregate root</returns>
        /// <remarks>
        /// Load never return null. It will always return an entity or throw an exception.
        /// It is permissible for Load to not hit the database (no query/select against a database) when it is called,
        /// it is free to return a proxy instead.
        /// </remarks>
        public virtual TEntity Load(TId id) {
            var entity = DbSet.Find(id);
            return entity ?? throw new InvalidOperationException($"Entity of type {typeof(TEntity).Name} with id {id} was not found.");
        }

        /// <summary>
        /// Return the persistent instance of the given entity class with the given identifier,
        /// assuming that the instnace exists.
        /// </summary>
        /// <param name="id">A valid identifier of an existing persistent instance of the class</param>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>Entity/aggregate root</returns>
        /// <remarks>
        /// Load never return null. It will always return an entity or throw an exception.
        /// It is permissible for Load to not hit the database (no query/select against a database) when it is called,
        /// it is free to return a proxy instead.
        /// </remarks>
        public virtual async Task<TEntity> LoadAsync(TId id, CancellationToken cancellationToken) {
            var entity = await DbSet.FindAsync([id], cancellationToken: cancellationToken).ConfigureAwait(false);
            return entity ?? throw new InvalidOperationException($"Entity of type {typeof(TEntity).Name} with id {id} was not found.");
        }

        #endregion

    }
}
