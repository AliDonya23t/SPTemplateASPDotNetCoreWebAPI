
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
                return new LoginResponseDto()
                {
                    Token = "",
                    User = null
                };
            }
            //if user was found generate JWT Token
            string token = CreateToken(user);

            //var refreshToken = GenerateRefreshToken();
            //SetRefreshToken(refreshToken, user.Id);

            LoginResponseDto loginResponseDTO = new LoginResponseDto()
            {
                Token = token,
                User = _mapper.Map<UserDto>(user),
                
                // Role = roles.FirstOrDefault()
            };
            //if (user.RefreshTokens.Any(a => a.IsActive))
            //{
            //    var activeRefreshToken = user.RefreshTokens.Where(a => a.IsActive == true).FirstOrDefault();
            //    //user.RefreshTokens.Token = activeRefreshToken.Token;
            //    //authenticationModel.RefreshTokenExpiration = activeRefreshToken.Expires;
            //}
            //else
            //{
            //    var refreshToken = GenerateRefreshToken();
            //    //authenticationModel.RefreshToken = refreshToken.Token;
            //    //authenticationModel.RefreshTokenExpiration = refreshToken.Expires;
            //    user.RefreshTokens.Add(refreshToken);
            //    _db.Update(user);
            //    _db.SaveChanges();
            //}
            return loginResponseDTO;

        }

        public async Task<UserDto> Register(RegisterationRequestDto registerationRequestDTO)
        {
            CreatePasswordHash(registerationRequestDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);
            User userNew = new()
            {
                UserName = registerationRequestDTO.UserName,
                //Name = registerationRequestDTO.Name,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };
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
        public async Task<AuthenticationModel> RefreshTokenAsync(string token)
        {
            var authenticationModel = new AuthenticationModel();

            var user = _db.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
            if (user is null)
            {
                authenticationModel.IsAuthenticated = false;
                authenticationModel.Message = $"Token did not match any users.";
                return authenticationModel;
            }

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive)
            {
                authenticationModel.IsAuthenticated = false;
                authenticationModel.Message = $"Token Not Active.";
                return authenticationModel;
            }

            //Revoke Current Refresh Token
            refreshToken.Revoked = DateTime.UtcNow;

            //Generate new Refresh Token and save to Database
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            _db.Update(user);
            _db.SaveChanges();

            //Generates new jwt
            authenticationModel.IsAuthenticated = true;
            authenticationModel.Token = CreateToken(user);
            //JwtSecurityToken jwtSecurityToken =  await CreateToken(user);
            //authenticationModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            //authenticationModel.Email = user.Email;
            authenticationModel.UserName = user.UserName;
            //var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            //authenticationModel.Roles = rolesList.ToList();
            authenticationModel.RefreshToken = newRefreshToken.Token;
            authenticationModel.RefreshTokenExpiration = newRefreshToken.Expires;
            return authenticationModel;
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8
                .GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
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
                Created = DateTime.Now
            };

            return refreshToken;
        }
        public async Task<AuthenticationModel> GetTokenAsync(UserDto model)
        {
            var authenticationModel = new AuthenticationModel();
            var user = _db.Users.FirstOrDefault(u => u.UserName.ToLower() == model.Username.ToLower());


            if (user == null)
            {
                authenticationModel.IsAuthenticated = false;
                authenticationModel.Message = $"No Accounts Registered with {model.Username}.";
                return authenticationModel;
            }
            if (VerifyPasswordHash(model.Password, user.PasswordHash, user.PasswordSalt) is true)
            {
                authenticationModel.IsAuthenticated = true;
                //JwtSecurityToken jwtSecurityToken = await CreateJwtToken(user);
                //authenticationModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                authenticationModel.Token =  CreateToken(user);
                //authenticationModel.Email = user.Email;
                authenticationModel.UserName = user.UserName;
                //var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
                //authenticationModel.Roles = rolesList.ToList();


                if (user.RefreshTokens.Any(a => a.IsActive))
                {
                    var activeRefreshToken = user.RefreshTokens.Where(a => a.IsActive == true).FirstOrDefault();
                    authenticationModel.RefreshToken = activeRefreshToken.Token;
                    authenticationModel.RefreshTokenExpiration = activeRefreshToken.Expires;
                }
                else
                {
                    var refreshToken = GenerateRefreshToken();
                    authenticationModel.RefreshToken = refreshToken.Token;
                    authenticationModel.RefreshTokenExpiration = refreshToken.Expires;
                    user.RefreshTokens.Add(refreshToken);
                    _db.Update(user);
                    _db.SaveChanges();
                }

                return authenticationModel;
            }
            authenticationModel.IsAuthenticated = false;
            authenticationModel.Message = $"Incorrect Credentials for user {user.UserName}.";
            return authenticationModel;
        }





    }   
}
