using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using ATLASDomain.DTO.Student;

namespace ATLAsBLL.Services.Student.Queries.GetStudentsByOferringCoures
{
    public record GetStudentsByOferringCouresQuery(int CourseOfferingID) : IQuery<IEnumerable<StudentDTO>>;
}
