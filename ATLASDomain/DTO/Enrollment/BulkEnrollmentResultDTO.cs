namespace ATLASDomain.DTO.Enrollment
{
    /// <summary>
    /// Bulk Enrollment Result Summary DTO.
    /// </summary>
    public class BulkEnrollmentResultDTO
    {
        /// <summary>
        /// Total number of data rows processed from the Excel file.
        /// </summary>
        public int TotalRowsProcessed { get; set; }

        /// <summary>
        /// Number of students successfully enrolled.
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Number of students skipped because they were already enrolled in the course offering.
        /// </summary>
        public int AlreadyEnrolledCount { get; set; }

        /// <summary>
        /// Number of Academic IDs from the file that were not found in the database.
        /// </summary>
        public int NotFoundCount { get; set; }

        /// <summary>
        /// Number of invalid or duplicate rows in the Excel file.
        /// </summary>
        public int InvalidRowsCount { get; set; }

        /// <summary>
        /// List of Academic IDs that were not found in the database.
        /// </summary>
        public List<string> NotFoundAcademicIds { get; set; } = new();

        /// <summary>
        /// List of Academic IDs that were already enrolled in the course offering.
        /// </summary>
        public List<string> AlreadyEnrolledAcademicIds { get; set; } = new();

        /// <summary>
        /// List of error descriptions for invalid or duplicate rows in the Excel file.
        /// </summary>
        public List<string> InvalidRows { get; set; } = new();
    }
}

