using ATLAsBLL.Features.Department.Comand.CreateDepartment;
using ATLAsBLL.Features.Department.Comand.DeleteDepartment;
using ATLAsBLL.Features.Department.Comand.UpdateDepartment;
using ATLAsBLL.Services.Department.Query;
using ATLAsBLL.Services.Instution.Command.Create;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ATLAS.Controllers.Institution
{    /// <summary>
    /// Institution Controller.
    /// </summary>
    [Route("api/Institution")]
    [ApiController]

    public class InstitutionController : Controller
    {
        /// <summary>
        /// _mediator.
        /// </summary>
        private readonly IMediator _mediator;
        public InstitutionController(IMediator mediator)
        {
            _mediator = mediator;
        }
       /// <summary>
        /// Create Institution.
        /// </summary>
        [HttpPost("CreateInstitution")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        // MED-16: creating, renaming or deleting an institution is a cross-tenant
        // action. A tenant-scoped Admin must not be able to reach another
        // institution's own record, so these three endpoints are SAdmin-only.
        [Authorize(Roles = "SAdmin")]
        [EnableRateLimiting("AdminUpdatePolicy")]
 
        public async Task<IActionResult> CreateInstitution([FromBody] CreateInstitutionDto institutionDto)
        {
            if (institutionDto != null)
            {
                var institution = await _mediator.Send(new CreateInstitutionCommand(institutionDto));
                if (institution == null)
                {
                    return BadRequest("Failed to create institution.");
                }
                // LOW-07: this is the one create endpoint with a matching GET, so
                // it can return a proper 201 with a Location header pointing at the
                // newly created institution.
                return CreatedAtAction(
                    nameof(GetInstitutionByID),
                    new { InstitutionID = institution.InstitutionID },
                    institution);
            }
            return BadRequest();
        }
        /// <summary>
        /// Update Institution.
        /// </summary>

        [HttpPut("UpdateInstitution")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        // MED-16: creating, renaming or deleting an institution is a cross-tenant
        // action. A tenant-scoped Admin must not be able to reach another
        // institution's own record, so these three endpoints are SAdmin-only.
        [Authorize(Roles = "SAdmin")]
        [EnableRateLimiting("AdminUpdatePolicy")]

        public async Task<IActionResult> UpdateInstitution([FromBody] UpdateInstitutionDto Dto)
        {
            try
            {
                if (Dto != null)
                {
                    var IsUpdatede = await _mediator.Send(new UpdateInstitutionCommand(Dto));
                    if (IsUpdatede)
                        return Ok("institution updated successfully.");

                }
                return BadRequest();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("institution not found.");
            }
        }


        /// <summary>
        /// Delete Institution.
        /// </summary>
        [HttpDelete("DeleteInstitution")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        // MED-16: creating, renaming or deleting an institution is a cross-tenant
        // action. A tenant-scoped Admin must not be able to reach another
        // institution's own record, so these three endpoints are SAdmin-only.
        [Authorize(Roles = "SAdmin")]
        [EnableRateLimiting("AdminSoftDeletePolicy")]

        public async Task<IActionResult> DeleteInstitution(int InstitutionID)
        {
            if (InstitutionID <= 0)
            {
                return BadRequest("Invalid institution ID.");
            }
            var ISDelete = await _mediator.Send(new DeleteInstitutionCommand(InstitutionID));
            if (!ISDelete)
            {
                return NotFound("institution not found.");
            }

            return Ok("institution deleted successfully.");
        }




     /// <summary>
        /// Get Institution.
        /// </summary>
        [HttpGet("GetInstitution")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminGetAllPolicy")]

   
        public async Task<IActionResult> GetInstitution()
        {
            var institution = await _mediator.Send(new GetAllInstitutionsQuery());

            // LOW-07: an empty result is a successful query, not a 404. The
            // message also said "No departments found" on an institution
            // endpoint, which was a copy-paste error.
            return Ok(institution ?? new List<InstitutionDto>());


        }
 /// <summary>
        /// Get Institution By ID.
        /// </summary>
        [HttpGet("GetInstitutionByID")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminGetAllPolicy")]

       
        public async Task<IActionResult> GetInstitutionByID(int InstitutionID)
        {
            var institution = await _mediator.Send(new GetInstitutionByIdQuery(InstitutionID));
            if (institution == null)
            {
                return NotFound("Institution not found.");
            }

            return Ok(institution);


        }



    }
}
