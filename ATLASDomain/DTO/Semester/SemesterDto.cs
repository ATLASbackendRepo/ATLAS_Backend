namespace ATLASDomain.DTO.Semester
{
    /// <summary>
    /// Semester DTO.
    /// </summary>
    public class SemesterDto
    {
        public int SemesterID { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public bool IsGradingClosed { get; set; }
        public int? InstitutionID { get; set; }
    }
}

