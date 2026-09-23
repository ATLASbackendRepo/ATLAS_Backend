using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.DTO.CourseOffring
{
    /// <summary>
    /// Course Offering DTO.
    /// </summary>
    public class CourseOfferingDTO

    {
        /// <summary>
        /// Course Offering Id.
        /// </summary>
        public int CourseOfferingId { get; set; }
        /// <summary>
        /// Course Offering Hours.
        /// </summary>
        public int CourseOfferingHours { get; set; }
        /// <summary>
        /// Course Name.
        /// </summary>
        public string CourseName { get; set; } = null!;
        /// <summary>
        /// Semester.
        /// </summary>
        public string Semester { get; set; } = null!;
        /// <summary>
        /// Doctorname.
        /// </summary>
        public string Doctorname { get; set; } = null!;

        /// <summary>
        /// Course Offering Grade.
        /// </summary>
        public decimal CourseOfferingGrade { get; set; }

    }
}
