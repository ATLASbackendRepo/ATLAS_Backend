using ATLASDomain.Entities;
using System.ComponentModel.DataAnnotations;
namespace ATLASDomain.DTO.Request;


public class UpdateGradeAppealRequestDto
{
    [Required]
    public enGradeAppealStatus Status { get; set; }

    [MaxLength(2000)]
    public string? DoctorResponse { get; set; }
}