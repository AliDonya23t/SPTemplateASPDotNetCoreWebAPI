using System.Text.Json.Serialization;

namespace SPTemplateASPDotNetCoreWebAPI.Models.DTO
{
    public class LoginResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //public string LastName { get; set; }
        public string Username { get; set; }
        public string JwtToken { get; set; }
        public string Role { get; set; }

        [JsonIgnore] // refresh token is returned in http only cookie
        public string RefreshToken { get; set; }

        public LoginResponseDto(User user, string jwtToken, string refreshToken)
        {
            Id = user.Id;
            //LastName = user.LastName;
            Name = user.Name;
            Username = user.UserName;
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
            Role = user.Role;
        }
    }
}
