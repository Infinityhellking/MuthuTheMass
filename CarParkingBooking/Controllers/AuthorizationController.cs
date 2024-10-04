﻿using CarParkingBooking.Services_Program;
using CarParkingBookingVM.Authorization;
using CarParkingBookingVM.Enums;
using CarParkingBookingVM.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ValidateCarParkingDetails.ValidateAuthorization;

namespace CarParkingBooking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorization authorization;

        public AuthorizationController(IAuthorization _authorization)
        {
            authorization = _authorization;
        }

        [HttpPost("signup")]
        public async Task<ActionResult> SignUp([FromBody] SignUpVM signUp)
        {

            var result = await authorization.UpsertLoginDetials(signUp);
            if (result is true)
            {
                return Ok(result);
            }
            else if (result is false)
            {
                return UnprocessableEntity(result);
            }
            else
            {
                return BadRequest(result);
            }



        }

        [HttpPost("userlogin")]
        public async Task<ActionResult> Login([FromBody] LoginVM loginVM)
        {
            var result = await authorization.VerifyUser(loginVM);
            if(result is not null)
            {
                var claims = new[]
            {
                new Claim(ClaimTypes.Name, result.UserName),
                new Claim(ClaimTypes.Role, AccessToUser.User) // Assign role
            };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSettingValues.JwtSecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: AppSettingValues.JwtIssuer,
                    audience: AppSettingValues.JwtAudience,
                    expires: DateTime.Now.AddMinutes(30),
                    claims: claims,
                    signingCredentials: creds);

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) , data = result });
            }
            else if(result is null)
            {
                return NotFound(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        //[HttpPost("alreadyexists")]
        //public async Task<ActionResult> AlreadyExists()
        //{

        //}
    }
}
