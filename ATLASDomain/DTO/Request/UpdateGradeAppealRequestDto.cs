using ATLASDomain.Entities;
using System.ComponentModel.DataAnnotations;
namespace ATLASDomain.DTO.Request;


/// <summary>
/// Update Grade Appeal Request Dto.
/// </summary>
public class UpdateGradeAppealRequestDto
{
    [Required]
    /// <summary>
    /// Status.
    /// </summary>
    public enGradeAppealStatus Status { get; set; }

    [MaxLength(2000)]
    /// <summary>
    /// Doctor Response.
    /// </summary>
    public string? DoctorResponse { get; set; }
}
