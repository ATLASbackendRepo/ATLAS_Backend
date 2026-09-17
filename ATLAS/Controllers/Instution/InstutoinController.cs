using ATLAsBLL.Features.Department.Comand.CreateDepartment;
using ATLAsBLL.Features.Department.Comand.DeleteDepartment;
using ATLAsBLL.Features.Department.Comand.UpdateDepartment;
using ATLAsBLL.Services.Department.Query;
using ATLAsBLL.Services.Instution.Command.Create;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ATLAS.Controllers.Instution
{
    [Route("api/Instutoin")]
    [ApiController]
    public class InstutoinController : Controller
    {
        private readonly IMediator _mediator;
        public InstutoinController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("CreateInstutoin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminUpdatePolicy")]
        public async Task<IActionResult> CreateInstutoin([FromBody] CreateInstitutionDto institutionDto)
        {
            if (institutionDto != null)
            {
                var institution = await _mediator.Send(new CreateInstitutionCommand(institutionDto));
                if (institution == null)
                {
                    return BadRequest("Failed to create institution.");
                }
                return Ok("institution created successfully.");
            }
            return BadRequest();
        }


        [HttpPut("UpdateInstutoin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminUpdatePolicy")]
        public async Task<IActionResult> UpdateInstutoin([FromBody] UpdateInstitutionDto Dto)
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



        [HttpDelete("DeleteInstutoin")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminSoftDeletePolicy")]
        public async Task<IActionResult> DeleteInstutoin(int InstutionID)
        {
            if (InstutionID <= 0)
            {
                return BadRequest("Invalid institution ID.");
            }
            var ISDelete = await _mediator.Send(new DeleteInstitutionCommand(InstutionID));
            if (!ISDelete)
            {
                return NotFound("institution not found.");
            }

            return Ok("institution deleted successfully.");
        }





        [HttpGet("GetInstutoin")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminGetAllPolicy")]

        public async Task<IActionResult> GetInstutoin()
        {
            var instution = await _mediator.Send(new GetAllInstitutionsQuery());
            if (instution == null || !instution.Any())
            {
                return NotFound("No departments found.");
            }

            return Ok(instution);


        }

        [HttpGet("GetInstutoinByID")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "SAdmin,Admin")]
        [EnableRateLimiting("AdminGetAllPolicy")]

        public async Task<IActionResult> GetInstutoinByID(int InstutionID)
        {
            var instution = await _mediator.Send(new GetInstitutionByIdQuery(InstutionID));
            if (instution == null )
            {
                return NotFound("No departments found.");
            }

            return Ok(instution);


        }



    }
}
