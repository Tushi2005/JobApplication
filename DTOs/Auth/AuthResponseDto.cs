namespace JobApplication.DTOs.Auth
{
    public class AuthResponseDto
    {
        public required string Token { get; set; }
        public required string Email { get; set; }
        public required string FullName { get; set; }
    }
}
