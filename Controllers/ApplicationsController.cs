using JobApplication.DTOs.Application;
using JobApplication.Services.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        private string GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in token.");
            return userIdClaim;
        }

        [HttpGet]
        public async Task<ActionResult<List<ResponseApplicationDto>>> GetAll()
        {
            string userId = GetCurrentUserId();
            var applications = await _applicationService.GetAllAsync(userId);
            return Ok(applications);
        }

        [HttpGet("companies")]
        public async Task<ActionResult<List<string>>> GetCompaniesByUser()
        {
            string userId = GetCurrentUserId();
            var companies = await _applicationService.GetCompaniesByUserAsync(userId);
            return Ok(companies);
        }

        [HttpGet("positions")]
        public async Task<ActionResult<List<string>>> GetPositionsByUser()
        {
            string userId = GetCurrentUserId();
            var positions = await _applicationService.GetPositionsByUserAsync(userId);
            return Ok(positions);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ResponseApplicationDto>> GetById(int id)
        {
            string userId = GetCurrentUserId();
            var application = await _applicationService.GetByIdAsync(id, userId);
            if (application == null) return NotFound();
            return application;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseApplicationDto>> Create([FromBody] CreateApplicationDto applicationDto)
        {
            string userId = GetCurrentUserId();
            var created = await _applicationService.CreateAsync(applicationDto, userId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id}, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ResponseApplicationDto>> Update(int id, [FromBody] UpdateApplicationDto updateDto)
        {
            string userId = GetCurrentUserId();

            var updated = await _applicationService.UpdateAsync(id, updateDto, userId);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            string userId = GetCurrentUserId();
            var success = await _applicationService.DeleteAsync(id, userId);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> PatchStatus(int id, [FromBody] JsonPatchDocument<ResponseApplicationDto> patchDoc)
        {
            string userId = GetCurrentUserId();
            var application = await _applicationService.GetByIdAsync(id, userId);

            if (application == null)
                return NotFound();

            patchDoc.ApplyTo(application, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _applicationService.PatchStatusAsync(id, application, userId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}