using ATLAsDCL.Data;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

public class UserRepository
    : GenericRepository<User>, IUserRepository
{
    public UserRepository(AtLASDbContext context)
        : base(context)
    {
    }

    public async Task<User?> GetByUserNameAsync(string userName)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.FullName == userName);
    }
    public async Task<User?> GetByUserIdAsync(int userId)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<bool> UserNameExistsAsync(string userName)
    {
        return await _dbSet.AnyAsync(u => u.FullName == userName);
    }
}