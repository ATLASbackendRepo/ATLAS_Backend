using System.Linq.Expressions;

namespace ATLASDomain.InterFaces.Repositories
{
    /// <summary>
    /// IGeneric Repository.
    /// </summary>
    public interface IGenericRepository<TEntity>
    where TEntity : class
    {


        /// <summary>
        /// Gets an entity by its primary key.
        /// </summary>
        /// <param name="id">Primary key value.</param>
        /// <param name="cancellationToken">Cancels the query.</param>
        /// <returns>The entity, or null when it does not exist.</returns>
        Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets every entity of this type.
        /// </summary>
        /// <param name="cancellationToken">Cancels the query.</param>
        /// <returns>All entities.</returns>
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets every entity that satisfies a predicate.
        /// </summary>
        /// <param name="predicate">Filter to apply in the database.</param>
        /// <param name="cancellationToken">Cancels the query.</param>
        /// <returns>The matching entities.</returns>
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the first entity that satisfies a predicate.
        /// </summary>
        /// <param name="predicate">Filter to apply in the database.</param>
        /// <param name="cancellationToken">Cancels the query.</param>
        /// <returns>The first match, or null when there is none.</returns>
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether any entity satisfies a predicate.
        /// </summary>
        /// <param name="predicate">Filter to apply in the database.</param>
        /// <param name="cancellationToken">Cancels the query.</param>
        /// <returns>True when at least one entity matches.</returns>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts every entity of this type.
        /// </summary>
        /// <param name="cancellationToken">Cancels the query.</param>
        /// <returns>The number of entities.</returns>
        Task<int> CountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts the entities that satisfy a predicate.
        /// </summary>
        /// <param name="predicate">Filter to apply in the database.</param>
        /// <param name="cancellationToken">Cancels the query.</param>
        /// <returns>The number of matching entities.</returns>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

      

        /// <summary>
        /// Stages a new entity for insertion. Call the unit of work to save.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">Cancels the operation.</param>
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stages a batch of new entities for insertion.
        /// </summary>
        /// <param name="entities">The entities to add.</param>
        /// <param name="cancellationToken">Cancels the operation.</param>
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);


        /// <summary>
        /// Stages an existing entity for update.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        void Update(TEntity entity);

        /// <summary>
        /// Stages an entity for deletion.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        void Delete(TEntity entity);

        /// <summary>
        /// Stages a batch of entities for deletion.
        /// </summary>
        /// <param name="entities">The entities to delete.</param>
        void DeleteRange(IEnumerable<TEntity> entities);

      
    }
}
