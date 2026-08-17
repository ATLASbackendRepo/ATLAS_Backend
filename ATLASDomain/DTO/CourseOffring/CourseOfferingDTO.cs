using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.DTO.CourseOffring
{
    public class CourseOfferingDTO

    {
        public int CourseOfferingId { get; set; }
        public int CourseOfferingHours { get; set; }
        public string CourseName { get; set; } = null!;
        public string Semester { get; set; } = null!;
        public string Doctorname { get; set; } = null!;

        public decimal CourseOfferingGrade { get; set; }

    }
}
