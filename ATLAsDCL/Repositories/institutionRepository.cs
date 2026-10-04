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
    /// <summary>
    /// institution Repository.
    /// </summary>
    public class institutionRepository : GenericRepository<Institution>, IinstitutionRepository
    {
        /// <summary>
        /// base.
        /// </summary>
        public institutionRepository(AtLASDbContext context) : base(context)
        {
        }
        /// <summary>
        /// Get Institution With Name Async.
        /// The lookup is read-only: a name that does not exist is reported as
        /// 0 so the caller can reject the row instead of silently creating an
        /// institution from a typo (MED-14).
        /// </summary>
        public async Task<int> GetInstitutionWithNameAsync(string institutionName, CancellationToken cancellationToken = default)
        {
            var institutionId = await _context.Institutions
                .AsNoTracking()
                .Where(i => i.name == institutionName)
                .Select(i => (int?)i.InstitutionID)
                .FirstOrDefaultAsync(cancellationToken);

            return institutionId ?? 0;
        }
   




        /// <summary>
        /// Get All Institutions Async.
        /// </summary>
        public async Task<List<InstitutionDto>> GetAllInstitutionsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Institutions
                .Select(i => new InstitutionDto
                {
                    InstitutionID = i.InstitutionID,
                    Name = i.name
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Get Institution By Id Async.
        /// </summary>
        public async Task<InstitutionDto> GetInstitutionByIdAsync( int id, CancellationToken cancellationToken = default)
        {
            return await _context.Institutions
                .Where(i => i.InstitutionID == id)
                .Select(i => new InstitutionDto
                {
                    InstitutionID = i.InstitutionID,
                    Name = i.name
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Create Institution Async.
        /// </summary>
        public async Task<InstitutionDto> CreateInstitutionAsync( CreateInstitutionDto dto, CancellationToken cancellationToken = default)
        {
            var institution = new Institution
            {
                name = dto.Name
            };

            _context.Institutions.Add(institution);
            await _context.SaveChangesAsync(cancellationToken);

            return new InstitutionDto
            {
                InstitutionID = institution.InstitutionID,
                Name = institution.name
            };
        }

        /// <summary>
        /// Update Institution Async.
        /// </summary>
        public async Task<bool> UpdateInstitutionAsync( UpdateInstitutionDto dto, CancellationToken cancellationToken = default)
        {
            var institution = await _context.Institutions.FindAsync(dto.InstitutionID, cancellationToken);
            if (institution == null) return false;

            institution.name = dto.Name;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Delete Institution Async.
        /// </summary>
        public async Task<bool> DeleteInstitutionAsync(int id, CancellationToken cancellationToken = default)
        {
            var institution = await _context.Institutions.FindAsync(id, cancellationToken);
            if (institution == null) return false;

            _context.Institutions.Remove(institution);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

    }
}
