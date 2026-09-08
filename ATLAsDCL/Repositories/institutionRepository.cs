using ATLAsDCL.Data;
using ATLASDomain.Entities;
using ATLASDomain.InterFaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLAsDCL.Repositories
{
    public class institutionRepository : GenericRepository<Institution>, IinstitutionRepository
    {
        public institutionRepository(AtLASDbContext context) : base(context)
        {
        }

        public async Task<int> GetInstitutionWithNameAsync(string institutionName)
        {
           
            var institutionId = await _context.Institutions
                .AsNoTracking()
                .Where(i => i.name == institutionName)
                .Select(i => (int?)i.InstitutionID)
                .FirstOrDefaultAsync();

            if (institutionId.HasValue)
            {
                return  institutionId.Value;
            }

           
            var newInstitution = new Institution
            {
                name = institutionName
            };

            _context.Institutions.Add(newInstitution);
            await _context.SaveChangesAsync();

            
            return newInstitution.InstitutionID;
        }

      
    }
}
