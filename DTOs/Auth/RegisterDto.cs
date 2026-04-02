namespace JobApplication.DTOs.Auth
{
    public class RegisterDto
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string Password { get; set; }
    }
}
