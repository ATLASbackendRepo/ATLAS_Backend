using ATLAsBLL.Features.Semesters.Commands.CreateSemester;
using ATLAsBLL.Features.Semesters.Commands.DeleteSemester;
using ATLAsBLL.Features.Semesters.Commands.UpdateSemester;
using ATLAsBLL.Features.Semesters.Queries.GetSemesterById;
using ATLAsBLL.Features.Semesters.Queries.GetSemesters;
using ATLASDomain.DTO.Semester;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
        /// Retrieves all semesters scoped to the caller's institution (or all for SAdmin).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<SemesterDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetSemesters(CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetSemestersQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a semester by ID, verifying tenant ownership.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(SemesterDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSemesterById([FromRoute] int id, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetSemesterByIdQuery(id), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new semester scoped to the caller's institution.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateSemester(
            [FromBody] CreateSemesterCommand command,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var semesterId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetSemesterById), new { id = semesterId }, new { semesterId });
        }

        /// <summary>
        /// Updates a semester, including toggling IsGradingClosed, with institution ownership verification.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
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
        /// Deletes a semester after verifying ownership and absence of active course offerings.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
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
