using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.DTO.CourseOffring
{
    public class CourseofferingDetailsDTO
    {
   
        public string C_Name { get; set; } = string.Empty;
        public short Credit_Hours { get; set; }

        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public decimal maxDegree { get; set; }

        public decimal studentdegree { get; set; }

       
    }
}
