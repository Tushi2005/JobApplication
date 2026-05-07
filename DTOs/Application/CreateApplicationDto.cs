using System.ComponentModel.DataAnnotations;

namespace JobApplication.DTOs.Application
{
    public class CreateApplicationDto: ApplicationBaseDto
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public CreateApplicationDto()
        {
            AppliedAt = DateTime.UtcNow;
            
        }
    }
}
