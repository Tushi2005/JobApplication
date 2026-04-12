using JobApplication.DTOs.Application;
using JobApplication.Models;

namespace JobApplication.Mappers
{
    public static class ApplicationMapper
    {
        public static ApplicationResponseDto ToDto(this Application a)
        {
            return new ApplicationResponseDto
            {
                Id = a.Id,
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
