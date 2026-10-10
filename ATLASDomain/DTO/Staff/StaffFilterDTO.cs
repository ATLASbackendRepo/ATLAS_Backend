using ATLASDomain.Entities;

namespace ATLASDomain.DTO.Staff
{
    /// <summary>
    /// Staff Filter DTO.
    /// </summary>
    /// <remarks>
    /// Inherits paging from <see cref="PagedFilter"/>, so the staff list can no
    /// longer be requested without a bounded page size (MED-12).
    /// </remarks>
    public class StaffFilterDTO : PagedFilter
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

        /// <summary>
        /// Institution Id for tenant isolation.
        /// </summary>
        public int? InstitutionId { get; set; }
    }
}