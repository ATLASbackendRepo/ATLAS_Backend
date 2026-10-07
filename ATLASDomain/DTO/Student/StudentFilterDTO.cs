namespace ATLASDomain.DTO.Student
{
    /// <summary>
    /// Student Filter DTO.
    /// </summary>
    /// <remarks>
    /// Inherits paging from <see cref="PagedFilter"/>, so the student list can no
    /// longer be requested without a bounded page size (MED-12).
    /// </remarks>
    public class StudentFilterDTO : PagedFilter
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
        /// Institution Id tenant filter.
        /// </summary>
        public int? InstitutionId { get; set; }
    }
}