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
    public async Task<TEntity?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// Get All Async.
    /// </summary>
    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>
    /// Find Async.
    /// </summary>
    public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    /// <summary>
    /// First Or Default Async.
    /// </summary>
    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    /// <summary>
    /// Any Async.
    /// </summary>
    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    /// <summary>
    /// Count Async.
    /// </summary>
    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    /// <summary>
    /// Count Async.
    /// </summary>
    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }


    /// <summary>
    /// Add Async.
    /// </summary>
    public async Task AddAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
    }

    /// <summary>
    /// Add Range Async.
    /// </summary>
    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await _dbSet.AddRangeAsync(entities);
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
