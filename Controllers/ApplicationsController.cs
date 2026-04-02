using JobApplication.DTOs.Application;
using JobApplication.Models;
using JobApplication.Services.Applications;
using Microsoft.AspNetCore.Mvc;

namespace JobApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ApplicationResponseDto>>> GetAll()
        {
            int userId = 1;

            var applications = await _applicationService.GetAllAsync(userId);

            var result = applications.Select(a => MapToDto(a)).ToList();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationResponseDto>> GetById(int id)
        {
            int userId = 1; // TODO: JWT-ből majd kiolvasni 

            var application = await _applicationService.GetByIdAsync(id, userId);

            if (application == null)
                return NotFound();

            return Ok(MapToDto(application));
        }

        [HttpPost]
        public async Task<ActionResult<ApplicationResponseDto>> Create([FromBody] CreateApplicationDto dto)
        {
            int userId = 1;

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

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApplicationResponseDto>> Update(int id, [FromBody] UpdateApplicationDto dto)
        {
            int userId = 1;

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

            if (updated == null)
                return NotFound();

            return Ok(MapToDto(updated));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int userId = 1;

            var success = await _applicationService.DeleteAsync(id, userId);

            if (!success)
                return NotFound();

            return NoContent();
        }

        private static ApplicationResponseDto MapToDto(Application a)
        {
            return new ApplicationResponseDto
            {
                Id = a.Id,
                UserId = a.UserId,
                CompanyName = a.CompanyName,
                Position = a.Position,
                Status = a.Status,
                AppliedAt = a.AppliedAt,
                InterviewAt = a.InterviewAt,
                JobUrl = a.JobUrl,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            };
        }
    }
}
