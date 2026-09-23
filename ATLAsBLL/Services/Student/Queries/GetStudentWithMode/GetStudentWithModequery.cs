using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATLASDomain.DTO.Student;
using MediatR;

namespace ATLAsBLL.Services.Student.Queries.GetStudentWithMode
{
    /// <summary>
    /// Get Student With Modequery.
    /// </summary>
    public record GetStudentWithModequery(int CourseOfferingID, int ModelID) : IQuery<IEnumerable<StudentGradeDto>>
    {
    }
}
