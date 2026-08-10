using MediatR;
using System.Collections.Generic;
using ATLASDomain.Entities;
using ATLASDomain.DTO.Student;

namespace ATLAsBLL.Services.Student.Queries.GetallStudent
{
  
    public record GetAllStudentsQuery() : IQuery<IEnumerable<StudentDTO>>;
}