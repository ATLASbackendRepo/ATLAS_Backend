using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.DTO.Student
{
    /// <summary>
    /// Student Grade Dto.
    /// </summary>
    public  class StudentGradeDto
    {
        /// <summary>
        /// Student Code.
        /// </summary>
        public string StudentCode { get; set; } = null!;
        /// <summary>
        /// Academic Id.
        /// </summary>
        public string AcademicId { get; set; } = null!;
        /// <summary>
        /// Studentname.
        /// </summary>
        public string Studentname { get; set; } = null!;

        /// <summary>
        /// Studentdegree.
        /// </summary>
        public  decimal Studentdegree { get; set; }
    }
}
