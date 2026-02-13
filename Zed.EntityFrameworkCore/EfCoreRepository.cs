using Microsoft.EntityFrameworkCore;

namespace Zed.EntityFrameworkCore {

    /// <summary>
    /// Base Entity Framework Core repository
    /// </summary>
    public abstract class EfCoreRepository {

        #region Fields and Properties

        /// <summary>
        /// DbContext
        /// </summary>
        protected readonly DbContext DbContext;

        #endregion

        #region Constructors and Init

        /// <summary>
        /// Creates Entity Framework Core repository
        /// </summary>
        /// <param name="dbContext">Entity Framework Core DbContext</param>
        protected EfCoreRepository(DbContext dbContext) {
            ArgumentNullException.ThrowIfNull(dbContext);

            this.DbContext = dbContext;
        }

        #endregion

        #region Methods

        #endregion

    }
}
