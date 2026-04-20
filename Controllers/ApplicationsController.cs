using JobApplication.DTOs.Application;
using JobApplication.Mappers;
using JobApplication.Models;
using JobApplication.Services.Applications;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<ActionResult<List<ApplicationResponseDto>>> GetAll()
        {
            string userId = GetCurrentUserId();
            var applications = await _applicationService.GetAllAsync(userId);
            return Ok(applications.Select(a => a.ToDto()).ToList());
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
        public async Task<ActionResult<ApplicationResponseDto>> GetById(int id)
        {
            string userId = GetCurrentUserId();
            var application = await _applicationService.GetByIdAsync(id, userId);
            if (application == null) return NotFound();
            return Ok(application.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<ApplicationResponseDto>> Create([FromBody] CreateApplicationDto dto)
        {
            string userId = GetCurrentUserId();

            var application = new Application
            {
                UserId = userId,
                CompanyName = dto.CompanyName,
                Position = dto.Position,
                AppliedAt = dto.AppliedAt,
                InterviewAt = dto.InterviewAt,
                JobUrl = dto.JobUrl,
                Notes = dto.Notes,
                Status = ApplicationStatus.Sent
            };

            var created = await _applicationService.CreateAsync(application);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApplicationResponseDto>> Update(int id, [FromBody] UpdateApplicationDto dto)
        {
            string userId = GetCurrentUserId();

            var application = new Application
            {
                CompanyName = dto.CompanyName,
                Position = dto.Position,
                Status = dto.Status,
                AppliedAt = dto.AppliedAt,
                InterviewAt = dto.InterviewAt,
                JobUrl = dto.JobUrl,
                Notes = dto.Notes
            };

            var updated = await _applicationService.UpdateAsync(id, application, userId);
            if (updated == null) return NotFound();
            return Ok(updated.ToDto());
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
        public async Task<IActionResult> PatchStatus(int id, [FromBody] PatchDto dto)
        {
            string userId = GetCurrentUserId();
            if (dto.Status == null) return BadRequest();

            var updated = await _applicationService.PatchStatusAsync(id, dto.Status.Value, userId);
            if (updated == null) return NotFound();
            return Ok(updated.ToDto());
        }
    }
}