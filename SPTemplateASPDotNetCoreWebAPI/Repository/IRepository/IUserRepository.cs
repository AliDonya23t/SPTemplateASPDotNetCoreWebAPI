using Microsoft.AspNetCore.Mvc;
using SPTemplateASPDotNetCoreWebAPI.Models;
using SPTemplateASPDotNetCoreWebAPI.Models.DTO;

namespace SPTemplateASPDotNetCoreWebAPI.Repository.IRepository
{
    public interface IUserRepository
    {
        string GetMyName();
        bool IsUniqueUser(string username);
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDTO);
        Task<UserDto> Register(RegisterationRequestDto registerationRequestDTO);
        Task<AuthenticationModel> GetTokenAsync(UserDto model);
        Task<AuthenticationModel> RefreshTokenAsync(string token);
        //void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        //RefreshToken GenerateRefreshToken();
        //void SetRefreshToken(RefreshToken newRefreshToken, int idUser);
        //string CreateToken(User user);
        //bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);


    }

}
