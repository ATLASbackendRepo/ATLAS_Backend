using ATLASDomain.InterFaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ATLAsDCL.Data;

/// <summary>
/// Generic Repository.
/// </summary>
public class GenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    /// _context.
    /// </summary>
    protected readonly  AtLASDbContext _context;
    /// <summary>
    /// _db Set.
    /// </summary>
    protected readonly DbSet<TEntity> _dbSet;

    public GenericRepository(AtLASDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    /// <summary>
    /// Get By Id Async.
    /// </summary>
    /// <param name="id">Primary key value.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object?[] { id }, cancellationToken);
    }

    /// <summary>
    /// Get All Async.
    /// </summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Find Async.
    /// </summary>
    /// <param name="predicate">Filter to apply in the database.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    public async Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// First Or Default Async.
    /// </summary>
    /// <param name="predicate">Filter to apply in the database.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Any Async.
    /// </summary>
    /// <param name="predicate">Filter to apply in the database.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Count Async.
    /// </summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Count Async.
    /// </summary>
    /// <param name="predicate">Filter to apply in the database.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(predicate, cancellationToken);
    }


    /// <summary>
    /// Add Async.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Add Range Async.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <summary>
    /// Update.
    /// </summary>
    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    /// <summary>
    /// Delete.
    /// </summary>
    public void Delete(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// Delete Range.
    /// </summary>
    public void DeleteRange(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
    }
}
