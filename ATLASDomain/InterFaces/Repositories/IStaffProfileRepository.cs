using ATLAS.Domain.Entities;
using ATLASDomain.InterFaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IStaffProfileRepository : IGenericRepository<StaffProfile>
{
    Task<StaffProfile?> GetByUserIdAsync(int userId);
 
}