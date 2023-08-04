using System.Text.Json.Serialization;

namespace SPTemplateASPDotNetCoreWebAPI.Models.DTO
{
    public class LoginResponseDto
    {
        //public UserDto User { get; set; }
        //public string Token { get; set; }
        //public string Role { get; set; }

        public int Id { get; set; }
        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        public string Username { get; set; }
        public string JwtToken { get; set; }

        [JsonIgnore] // refresh token is returned in http only cookie
        public string RefreshToken { get; set; }

        public LoginResponseDto(User user, string jwtToken, string refreshToken)
        {
            Id = user.Id;
            //FirstName = user.FirstName;
            //LastName = user.LastName;
            Username = user.UserName;
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
        }
    }
}
