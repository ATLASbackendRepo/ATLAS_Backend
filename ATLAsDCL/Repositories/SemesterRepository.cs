using ATLAsDCL.Data;
using ATLASDomain.Entities;
using ATLASDomain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

public class SemesterRepository
    : GenericRepository<Semester>, ISemesterRepository
{
    public SemesterRepository(AtLASDbContext context)
        : base(context)
    {
    }

}