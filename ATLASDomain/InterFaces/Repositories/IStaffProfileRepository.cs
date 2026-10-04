using ATLAS.Domain.Entities;
using ATLASDomain.InterFaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// IStaff Profile Repository.
/// </summary>
public interface IStaffProfileRepository : IGenericRepository<StaffProfile>
{
    /// <summary>
    /// Gets the staff profile belonging to a user.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <returns>The staff profile, or null when the user has none.</returns>
    Task<StaffProfile?> GetByUserIdAsync(int userId);
 
}
