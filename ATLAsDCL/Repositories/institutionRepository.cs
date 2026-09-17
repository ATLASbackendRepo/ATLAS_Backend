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
   




        public async Task<List<InstitutionDto>> GetAllInstitutionsAsync()
        {
            return await _context.Institutions
                .Select(i => new InstitutionDto
                {
                    InstitutionID = i.InstitutionID,
                    Name = i.name
                })
                .ToListAsync();
        }

        public async Task<InstitutionDto> GetInstitutionByIdAsync( int id)
        {
            return await _context.Institutions
                .Where(i => i.InstitutionID == id)
                .Select(i => new InstitutionDto
                {
                    InstitutionID = i.InstitutionID,
                    Name = i.name
                })
                .FirstOrDefaultAsync();
        }

        public async Task<InstitutionDto> CreateInstitutionAsync( CreateInstitutionDto dto)
        {
            var institution = new Institution
            {
                name = dto.Name
            };

            _context.Institutions.Add(institution);
            await _context.SaveChangesAsync();

            return new InstitutionDto
            {
                InstitutionID = institution.InstitutionID,
                Name = institution.name
            };
        }

        public async Task<bool> UpdateInstitutionAsync( UpdateInstitutionDto dto)
        {
            var institution = await _context.Institutions.FindAsync(dto.InstitutionID);
            if (institution == null) return false;

            institution.name = dto.Name;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteInstitutionAsync(int id)
        {
            var institution = await _context.Institutions.FindAsync(id);
            if (institution == null) return false;

            _context.Institutions.Remove(institution);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
