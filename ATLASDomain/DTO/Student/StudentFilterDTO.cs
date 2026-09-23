namespace ATLASDomain.DTO.Student
{
    /// <summary>
    /// Student Filter DTO.
    /// </summary>
    public class StudentFilterDTO
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
