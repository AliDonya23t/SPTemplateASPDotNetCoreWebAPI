namespace SPTemplateASPDotNetCoreWebAPI.Models.DTO
{
    public class RegisterationRequestDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
