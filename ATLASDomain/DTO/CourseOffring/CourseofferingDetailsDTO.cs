using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.DTO.CourseOffring
{
    /// <summary>
    /// Courseoffering Details DTO.
    /// </summary>
    public class CourseofferingDetailsDTO
    {
   
        /// <summary>
        /// C_Name.
        /// </summary>
        public string C_Name { get; set; } = string.Empty;
        /// <summary>
        /// Credit_Hours.
        /// </summary>
        public int Credit_Hours { get; set; }

        /// <summary>
        /// Module Id.
        /// </summary>
        public int ModuleId { get; set; }
        /// <summary>
        /// Module Name.
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;
        /// <summary>
        /// max Degree.
        /// </summary>
        public decimal maxDegree { get; set; }

        /// <summary>
        /// studentdegree.
        /// </summary>
        public decimal studentdegree { get; set; }

       
    }
}
