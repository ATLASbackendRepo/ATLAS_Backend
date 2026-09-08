namespace ATLASDomain.DTO.AssessmentModule
{
    public class CreateModeDTO
    {
        public int CourseOfferingId { get; set; }
        public string? ModuleName { get; set; }
        public decimal MaxDegree { get; set; }
    }
}
