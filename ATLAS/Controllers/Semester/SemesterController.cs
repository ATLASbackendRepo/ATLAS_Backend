using ATLAsBLL.Features.Semesters.Commands.CreateSemester;
using ATLAsBLL.Features.Semesters.Commands.DeleteSemester;
using ATLAsBLL.Features.Semesters.Commands.UpdateSemester;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace ATLAS.Controllers.Semester
{
    /// <summary>
    /// Semester Management Controller.
    /// </summary>
    [Route("api/Semesters")]
    [ApiController]
    [Authorize(Roles = "SAdmin,Admin")]
    public class SemesterController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SemesterController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a new semester.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSemester(
            [FromBody] CreateSemesterCommand command,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var semesterId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(CreateSemester), new { id = semesterId }, new { semesterId });
        }

        /// <summary>
        /// Updates a semester, including toggling IsGradingClosed.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSemester(
            [FromRoute] int id,
            [FromBody] UpdateSemesterRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new UpdateSemesterCommand(id, request.SemesterName, request.AcademicYear, request.IsGradingClosed);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
            {
                return NotFound($"Semester with ID {id} was not found.");
            }

            return Ok(new { message = "Semester updated successfully." });
        }

        /// <summary>
        /// Deletes a semester.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSemester(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var command = new DeleteSemesterCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result)
            {
                return NotFound($"Semester with ID {id} was not found.");
            }

            return Ok(new { message = "Semester deleted successfully." });
        }
    }

    /// <summary>
    /// Update Semester Request DTO.
    /// </summary>
    public record UpdateSemesterRequest(string SemesterName, string AcademicYear, bool IsGradingClosed);
}

