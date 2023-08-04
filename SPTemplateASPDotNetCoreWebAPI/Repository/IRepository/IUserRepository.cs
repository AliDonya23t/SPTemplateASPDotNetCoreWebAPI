using Microsoft.AspNetCore.Mvc;
using SPTemplateASPDotNetCoreWebAPI.Models;
using SPTemplateASPDotNetCoreWebAPI.Models.DTO;
using System.Net;

namespace SPTemplateASPDotNetCoreWebAPI.Repository.IRepository
{
    public interface IUserRepository
    {
        User GetById(int id);
        string GetMyName();
        bool IsUniqueUser(string username);
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDTO);
        Task<UserDto> Register(RegisterationRequestDto registerationRequestDTO);
        Task<LoginResponseDto> RefreshTokenAsync(string token );
        bool RevokeToken(string token);
        //void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        //RefreshToken GenerateRefreshToken();
        //void SetRefreshToken(RefreshToken newRefreshToken, int idUser);
        //string CreateToken(User user);
        //bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);


    }

}
