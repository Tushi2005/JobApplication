using JobApplication.Models;

namespace JobApplication.DTOs.Application
{
    public class PatchApplicationStatusDto
    {
        public ApplicationStatus? Status { get; set; }
    }
}
