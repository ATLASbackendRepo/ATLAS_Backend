namespace ATLASDomain.DTO.Student
{
    /// <summary>
    /// Create Student DTO.
    /// </summary>
    public class CreateStudentDTO
    {
        /// <summary>
        /// Full Name.
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        /// <summary>
        /// Academic Id.
        /// </summary>
        public string AcademicId { get; set; } = string.Empty;
        /// <summary>
        /// Academic Level.
        /// </summary>
        public int AcademicLevel { get; set; }
        /// <summary>
        /// Department Id.
        /// </summary>
        public int DepartmentId { get; set; }
        /// <summary>
        /// Institution Id.
        /// </summary>
        public int InstitutionId { get; set; }
    }
}
