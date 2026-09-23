namespace ATLASDomain.DTO.CourseGrade
{
    /// <summary>
    /// Get Students Paged Query Model.
    /// </summary>
    public class GetStudentsPagedQueryModel
    {
        /// <summary>
        /// Max Page Size.
        /// </summary>
        public const int MaxPageSize = 100;
        /// <summary>
        /// Course Offering Id.
        /// </summary>
        public int CourseOfferingId { get; set; }
        /// <summary>
        /// Page Number.
        /// </summary>
        public int PageNumber { get; set; } = 1;     
        /// <summary>
        /// Page Size.
        /// </summary>
        public int PageSize { get; set; } = 9;      
        /// <summary>
        /// Search Term.
        /// </summary>
        public string? SearchTerm { get; set; }      
    }


    /// <summary>
    /// Student Module Grade Dto.
    /// </summary>
    public class StudentModuleGradeDto
    {
        /// <summary>
        /// Module Id.
        /// </summary>
        public int ModuleId { get; set; }
        /// <summary>
        /// Module Name.
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;
        /// <summary>
        /// Student Degree.
        /// </summary>
        public double? StudentDegree { get; set; } 
        /// <summary>
        /// Max Degree.
        /// </summary>
        public double MaxDegree { get; set; }
    }

    /// <summary>
    /// Student Card Dto.
    /// </summary>
    public class StudentCardDto
    {
        /// <summary>
        /// Student Id.
        /// </summary>
        public int StudentId { get; set; }
        /// <summary>
        /// Student Name.
        /// </summary>
        public string StudentName { get; set; } = string.Empty;
        /// <summary>
        /// Student Code.
        /// </summary>
        public string StudentCode { get; set; } = string.Empty;
        /// <summary>
        /// Academic Id.
        /// </summary>
        public string AcademicId { get; set; } = string.Empty;

        /// <summary>
        /// Enrollment Id.
        /// </summary>
        public int EnrollmentId { get; set; }
        /// <summary>
        /// Grades.
        /// </summary>
        public List<StudentModuleGradeDto> Grades { get; set; } = new();
    }

    /// <summary>
    /// Paged Result Dto.
    /// </summary>
    public class PagedResultDto<T>
    {
        /// <summary>
        /// Items.
        /// </summary>
        public List<T> Items { get; set; } = new();
        /// <summary>
        /// Page Number.
        /// </summary>
        public int PageNumber { get; set; }
        /// <summary>
        /// Page Size.
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// Total Count.
        /// </summary>
        public int TotalCount { get; set; }
        /// <summary>
        /// Total Pages.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        /// <summary>
        /// Has Next Page.
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;
        /// <summary>
        /// Has Previous Page.
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
    }






}
