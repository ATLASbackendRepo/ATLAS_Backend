using ATLAS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.InterFaces.Repositories
{
    public interface IStudentProfileRepository :IGenericRepository<StudentProfile>
    {
        Task<StudentProfile?> GetByStudentIdAsync(int StudentId);
      

    }
}