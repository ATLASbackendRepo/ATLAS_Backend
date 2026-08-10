using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.DTO.Student
{
    public  class StudentGradeDto
    {
        public string StudentCode { get; set; } = null!;
        public string AcademicId { get; set; } = null!;
        public string Studentname { get; set; } = null!;

        public  decimal Studentdegree { get; set; }
    }
}
