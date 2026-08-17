using ATLAsBLL.Services.profile.Staff.Query;
using ATLASDomain.DTO.Profile;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ATLAS.API.Controllers.Profile
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProfilesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("STaffProfile/{userId}")]
        [ProducesResponseType(typeof(StaffProfileResponseDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]

        public async Task<ActionResult<StaffProfileResponseDto>> GetByUserId(int userId)

        {
            if (userId <= 0)
                return BadRequest("Invalid UserId");

            var result = await _mediator.Send(new GetStaffProfileByUserIdQuery(userId));

            if (result == null)
                return NotFound($"No  profile found ");

            return Ok(result);
        }
    }
}