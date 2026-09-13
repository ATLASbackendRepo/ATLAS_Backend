using ATLASDomain.Entities;

namespace ATLASDomain.DTO.Staff
{
    public class StaffFilterDTO
    {
        public string? Search { get; set; }
        public int? DepartmentId { get; set; }
        public int? Level { get; set; }

      
        public bool? Status { get; set; }
    }
}
