namespace ATLASDomain.DTO.AssessmentModule
{
    /// <summary>
    /// Update Mode DTO.
    /// </summary>
    public class UpdateModeDTO
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
