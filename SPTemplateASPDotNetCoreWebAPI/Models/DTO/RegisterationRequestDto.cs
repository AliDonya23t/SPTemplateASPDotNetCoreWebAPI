using System.ComponentModel.DataAnnotations;

namespace SPTemplateASPDotNetCoreWebAPI.Models.DTO
{
    public class RegisterationRequestDto
    {
        [Required]
        [MinLength(3)]
        public string UserName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } ="Member";
    }
}
