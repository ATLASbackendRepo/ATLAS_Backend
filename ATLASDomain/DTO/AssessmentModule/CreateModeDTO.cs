namespace ATLASDomain.DTO.AssessmentModule
{
    /// <summary>
    /// Create Mode DTO.
    /// </summary>
    public class CreateModeDTO
    {
        /// <summary>
        /// Course Offering Id.
        /// </summary>
        public int CourseOfferingId { get; set; }
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
