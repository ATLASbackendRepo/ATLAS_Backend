using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATLASDomain.DTO.AssessmentModule
{
    public class AssessmentModelDisplayDTO
    {
        public int ModuleId { get; set; }
        public string? ModuleName { get; set; }
        public decimal MaxDegree { get; set; }
    }
}
