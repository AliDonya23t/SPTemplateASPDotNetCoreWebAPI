
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SPTemplateASPDotNetCoreWebAPI.Data;
using SPTemplateASPDotNetCoreWebAPI.Models;
using SPTemplateASPDotNetCoreWebAPI.Models.DTO;
using SPTemplateASPDotNetCoreWebAPI.Repository.IRepository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Web;
using AutoMapper;
using Azure;
using Microsoft.EntityFrameworkCore;
using Azure.Core;
using System.Net;

namespace SPTemplateASPDotNetCoreWebAPI.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserRepository(ApplicationDbContext db, IConfiguration configuration,
            IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        public User GetById(int id)
        {
            return _db.Users.Find(id);
        }
        public string GetMyName()
        {
            var result = string.Empty;
            if (_httpContextAccessor.HttpContext != null)
            {
                result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            }
            return result;
        }
        public bool IsUniqueUser(string username)
        {
            var user = _db.Users.FirstOrDefault(x => x.UserName == username);
            if (user == null)
            {
                return true;
            }
            return false;
        }
        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDTO)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());

            if (user is null || VerifyPasswordHash(loginRequestDTO.Password, user.PasswordHash, user.PasswordSalt) is false)
            {
                return null;
            }
            //if user was found generate JWT Token
            string token = CreateToken(user);
            var refreshToken = GenerateRefreshToken();
            // save refresh token
            user.RefreshTokens.Add(refreshToken);
            _db.Update(user);
            _db.SaveChanges();

            return new LoginResponseDto(user, token, refreshToken.Token);
 
        }

        public async Task<UserDto> Register(RegisterationRequestDto registerationRequestDTO)
        {
            CreatePasswordHash(registerationRequestDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);
            
            User userNew = new()
            {
                UserName = registerationRequestDTO.UserName,
                Name = registerationRequestDTO.Name,
                //Role = registerationRequestDTO.Role,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };
            if (registerationRequestDTO.Role.ToLower() == "admin")
                userNew.Role = "Admin";
            else
                userNew.Role = "Member";

            try
            {

                await _db.AddAsync(userNew);
                await _db.SaveChangesAsync();

                return _mapper.Map<UserDto>(userNew);

            }
            catch (Exception e)
            {

            }

            return new UserDto();
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        public async Task<LoginResponseDto> RefreshTokenAsync(string token)
        {
            var user = _db.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user is null) return null;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive) return null;
  
            // replace old refresh token with a new one and save
            var newRefreshToken = GenerateRefreshToken();
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            user.RefreshTokens.Add(newRefreshToken);
            _db.Update(user);
            _db.SaveChanges();

            // generate new jwt
            var jwtToken = CreateToken(user);

            return new LoginResponseDto(user, jwtToken, newRefreshToken.Token);
        }
        public bool RevokeToken(string token)
        {
            var user = _db.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            // return false if no user found with token
            if (user == null) return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            // return false if token is not active
            if (!refreshToken.IsActive) return false;

            // revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            _db.Update(user);
            _db.SaveChanges();

            return true;
        }
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8
                .GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            //For check pass
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddMinutes(60),
                Created = DateTime.Now,
            };

            return refreshToken;
        }


    }   
}
