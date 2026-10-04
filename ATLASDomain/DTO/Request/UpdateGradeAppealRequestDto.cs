using ATLASDomain.Entities;
using System.ComponentModel.DataAnnotations;
namespace ATLASDomain.DTO.Request;


/// <summary>
/// Update Grade Appeal Request Dto.
/// </summary>
public class UpdateGradeAppealRequestDto
{
    /// <summary>
    /// Status.
    /// </summary>
    [Required]
    public enGradeAppealStatus Status { get; set; }

    /// <summary>
    /// Doctor Response.
    /// </summary>
    [MaxLength(2000)]
    public string? DoctorResponse { get; set; }
}
