using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByUserNameAsync(string userName);
    Task<User?> GetByUserIdAsync(int userId);

    Task<User?> GetUserByAcademicIDAsync(string academicID);
    Task<bool> UserNameExistsAsync(string userName);

}