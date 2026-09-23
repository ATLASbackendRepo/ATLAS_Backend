using ATLASDomain.Entities;

namespace ATLASDomain.DTO.Staff
{
    /// <summary>
    /// Staff Filter DTO.
    /// </summary>
    public class StaffFilterDTO
    {
        /// <summary>
        /// Search.
        /// </summary>
        public string? Search { get; set; }
        /// <summary>
        /// Department Id.
        /// </summary>
        public int? DepartmentId { get; set; }
        /// <summary>
        /// Level.
        /// </summary>
        public int? Level { get; set; }

      
        /// <summary>
        /// Status.
        /// </summary>
        public bool? Status { get; set; }
    }
}
