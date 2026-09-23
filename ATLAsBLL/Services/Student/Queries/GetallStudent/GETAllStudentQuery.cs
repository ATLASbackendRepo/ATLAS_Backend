using MediatR;
using System.Collections.Generic;
using ATLASDomain.Entities;
using ATLASDomain.DTO.Student;

namespace ATLAsBLL.Services.Student.Queries.GetallStudent
{
  
    /// <summary>
    /// Get All Students Query.
    /// </summary>
    public record GetAllStudentsQuery(StudentFilterDTO  StudentFilter) : IQuery<IEnumerable<StudentDTO>>;
}
