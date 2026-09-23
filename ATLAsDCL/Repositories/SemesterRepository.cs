using ATLAsDCL.Data;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Semester Repository.
/// </summary>
public class SemesterRepository
    : GenericRepository<Semester>, ISemesterRepository
{
    public SemesterRepository(AtLASDbContext context)
        : base(context)
    {
    }

}
