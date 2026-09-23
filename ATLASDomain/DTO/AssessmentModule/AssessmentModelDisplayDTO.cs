using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.DTO.AssessmentModule
{
    /// <summary>
    /// Assessment Model Display DTO.
    /// </summary>
    public class AssessmentModelDisplayDTO
    {
        /// <summary>
        /// Module Id.
        /// </summary>
        public int ModuleId { get; set; }
        /// <summary>
        /// Module Name.
        /// </summary>
        public string? ModuleName { get; set; }
        /// <summary>
        /// Max Degree.
        /// </summary>
        public decimal MaxDegree { get; set; }
    }
}
