using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SPTemplateASPDotNetCoreWebAPI.Models;
using SPTemplateASPDotNetCoreWebAPI.Models.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using SPTemplateASPDotNetCoreWebAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http.HttpResults;
using System;

namespace SPTemplateASPDotNetCoreWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        
        private readonly IUserRepository _userRepo;
        protected APIResponse _response;
        public AuthController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
            _response = new();
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]

        public ActionResult<string> GetMe()
        {
            var userName = _userRepo.GetMyName();
            return Ok(userName);
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterationRequestDto model)
        {
            bool ifUserNameUnique = _userRepo.IsUniqueUser(model.UserName);
            if (!ifUserNameUnique)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username already exists");
                return BadRequest(_response);
            }

            var user = await _userRepo.Register(model);
            if (user is null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error while registering");
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            return Ok(_response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login( LoginRequestDto model)
        {
            var loginResponse = await _userRepo.Login(model);
            if (loginResponse is null /*|| string.IsNullOrEmpty(loginResponse.JwtToken)*/)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username or password is incorrect");
                return BadRequest(_response);
            }
            SetRefreshTokenInCookie(loginResponse.RefreshToken);
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = loginResponse;
            return Ok(_response);
        }

      
        [HttpPost("refresh-token")]
        // login with token  then refresh, now you have to enter with the refreshed token next time
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var RefreshTokenresponse = await _userRepo.RefreshTokenAsync(refreshToken);
            if (RefreshTokenresponse is null )
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Error while RefreshToken");
                return BadRequest(_response);
            }
            if (!string.IsNullOrEmpty(RefreshTokenresponse.RefreshToken))
            {
                SetRefreshTokenInCookie(RefreshTokenresponse.RefreshToken);
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = RefreshTokenresponse;
            }
            return Ok(_response);
        }
        [HttpGet("{id}/refresh-tokens")]
        public IActionResult GetRefreshTokens(int id)
        {
            var user = _userRepo.GetById(id);
            if (user == null) return NotFound();

            return Ok(user.RefreshTokens);
        }

        [HttpPost("revoke-token")]
        public IActionResult RevokeToken([FromBody] RevokeTokenRequest model)
        {
            // accept token from request body or cookie
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Token is required");
                return BadRequest(_response);
            }

            var response = _userRepo.RevokeToken(token);

            if (!response)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Token not found");
                return BadRequest(_response);
            }
            else { 
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = response;
            }
            return Ok(_response);
        }
        private void SetRefreshTokenInCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(60),
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
        //private string ipAddress()
        //{
        //    if (Request.Headers.ContainsKey("X-Forwarded-For"))
        //        return Request.Headers["X-Forwarded-For"];
        //    else
        //        return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        //}

    }
}
