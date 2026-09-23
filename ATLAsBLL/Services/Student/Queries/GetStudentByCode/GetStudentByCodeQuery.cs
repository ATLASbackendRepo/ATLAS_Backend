using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATLASDomain.DTO.Student;

namespace ATLAsBLL.Services.Student.Queries.GetStudentByCode
{
    /// <summary>
    /// Get Student By Code Query.
    /// </summary>
    public record GetStudentByCodeQuery(string StudentCode) : IQuery<StudentDTO>;

}
