using Microsoft.AspNetCore.Identity;

namespace JobApplication.Models
{
    public class ApplicationUser: IdentityUser
    {
        public required string FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
