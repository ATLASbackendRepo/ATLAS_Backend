using ATLASDomain.DTO.Staff;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;

namespace ATLASDomain.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByUserNameAsync(string userName);
    Task<User?> GetByUserIdAsync(int userId);
     Task<IEnumerable<StaffDto>> GetAllStaffAsync(enUserRole role, StaffFilterDTO filter);

    Task<User?> GetUserByAcademicIDAsync(string academicID);
    Task<bool> UserNameExistsAsync(string userName);

}