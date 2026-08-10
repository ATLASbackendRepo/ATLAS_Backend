using MediatR;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATLASDomain.DTO.Student;

namespace ATLAsBLL.Services.Student.Queries.GetStudentsByLevel
{
    public record GetStudentsByLevelQuery(int Level) : IQuery<IEnumerable<StudentDTO>>;
}
