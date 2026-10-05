using ATLASDomain.DTO.Request;
using ATLASDomain.Entities;

/// <summary>
/// IGrade Appeal Request Repository.
/// </summary>
public interface IGradeAppealRequestRepository
{
    /// <summary>
    /// Gets a single grade appeal request.
    /// </summary>
    /// <param name="requestId">Appeal request identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The request, or null when it does not exist.</returns>
    Task<GradeAppealRequestDto?> GetByIdAsync(int requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets every appeal request raised by a student.
    /// </summary>
    /// <param name="studentId">Student user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The student's appeal requests.</returns>
    Task<List<GradeAppealRequestDto>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets every appeal request addressed to a doctor.
    /// </summary>
    /// <param name="doctorUserId">Doctor user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The appeal requests awaiting that doctor.</returns>
    Task<List<GradeAppealRequestDto>> GetByDoctorIdAsync(int doctorUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a student already has an unanswered appeal for a module.
    /// </summary>
    /// <param name="studentId">Student user identifier.</param>
    /// <param name="assessmentModuleId">Assessment module identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when an open appeal already exists.</returns>
    Task<bool> HasOpenRequestAsync(
        int studentId,
        int assessmentModuleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a new appeal request raised by a student.
    /// </summary>
    /// <param name="dto">The appeal details.</param>
    /// <param name="studentId">Student user identifier raising the appeal.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The identifier of the created request.</returns>
    Task<int> AddAsync(
     CreateGradeAppealRequestDto dto,
     int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a doctor's answer to an appeal request.
    /// </summary>
    /// <param name="requestId">Appeal request identifier.</param>
    /// <param name="doctorUserId">Doctor user identifier answering.</param>
    /// <param name="dto">The answer and the resulting grade.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when the request was found and updated.</returns>
    Task<bool> UpdateAsync(
    int requestId,
    int doctorUserId,
    UpdateGradeAppealRequestDto dto, CancellationToken cancellationToken = default);

}
