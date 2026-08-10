namespace ATLASDomain.DTO.CourseGrade
{
    public class GetStudentsPagedQueryModel
    {
        public const int MaxPageSize = 100;
        public int CourseOfferingId { get; set; }
        public int PageNumber { get; set; } = 1;     
        public int PageSize { get; set; } = 9;      
        public string? SearchTerm { get; set; }      
    }


    public class StudentModuleGradeDto
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public double? StudentDegree { get; set; } 
        public double MaxDegree { get; set; }
    }

    public class StudentCardDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string AcademicId { get; set; } = string.Empty;

        public int EnrollmentId { get; set; }
        public List<StudentModuleGradeDto> Grades { get; set; } = new();
    }

    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }






}
