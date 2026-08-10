using ATLASDomain.Entities;

public interface IGradeAppealRequestRepository
{
    Task<GradeAppealRequestDto?> GetByIdAsync(int requestId);

    Task<List<GradeAppealRequestDto>> GetByStudentIdAsync(int studentId);

    Task<List<GradeAppealRequestDto>> GetByDoctorIdAsync(int doctorUserId);

    Task<bool> HasOpenRequestAsync(
        int studentId,
        int assessmentModuleId);

    Task<int> AddAsync(
     CreateGradeAppealRequestDto dto,
     int studentId);
    Task<bool> UpdateAsync(
    int requestId,
    int doctorUserId,
    UpdateGradeAppealRequestDto dto);

}