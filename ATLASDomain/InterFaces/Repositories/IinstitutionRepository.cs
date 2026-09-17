using ATLASDomain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.InterFaces.Repositories
{
    public interface IinstitutionRepository : IGenericRepository<Institution>
    {

      Task<int> GetInstitutionWithNameAsync(string institutionName);

        Task<List<InstitutionDto>> GetAllInstitutionsAsync();
        Task<InstitutionDto> GetInstitutionByIdAsync(int id);
        Task<InstitutionDto> CreateInstitutionAsync(CreateInstitutionDto dto);
        Task<bool> UpdateInstitutionAsync(UpdateInstitutionDto dto);
        Task<bool> DeleteInstitutionAsync(int id);


    }
}
