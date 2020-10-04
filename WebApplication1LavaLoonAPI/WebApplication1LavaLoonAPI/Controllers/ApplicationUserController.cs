using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebApplication1LavaLoonAPI.Models;
using WebApplication1LavaLoonAPI.Models.ViewModel;

namespace WebApplication1LavaLoonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUserController : ControllerBase
    {
        UserManager<ApplicationUser> _usermanager;
        SignInManager<ApplicationUser> _signinmanager;
        AuthenticationContext _db;

       
        public ApplicationUserController(UserManager<ApplicationUser> usermanager, SignInManager<ApplicationUser> signinmanager, AuthenticationContext db)
        {
            _db = db;
            _usermanager = usermanager;
            _signinmanager = signinmanager;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<object> PostApplicationUser(ApplicationUserViewModel model)
        {

            
            var user = new ApplicationUser()
            {
                Email = model.Email,
                FullName = model.FullName,
                UserName = model.UserName
            };
            try
            {
                var result = await _usermanager.CreateAsync(user, model.Password);
                return Ok(result);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var user = await _usermanager.FindByNameAsync(model.UserName);
            if (user !=null && await _usermanager.CheckPasswordAsync(user,model.Password))
            {
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("UserID", user.Id.ToString())

                    }),
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1234567890123456")), SecurityAlgorithms.HmacSha256Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);
                return Ok(new { token });
            }
            else
            {
                return BadRequest(new { message = "UserName Or Password InCOrrect" });
            }
        }
        [HttpGet]
        [Route("UserProfile")]
        [Authorize]
        public async Task<object> GetUserProfile()
        {
            string UserId = User.Claims.FirstOrDefault(a => a.Type == "UserID").Value;
            var user = await _usermanager.FindByIdAsync(UserId);
            return new
            {
                user.FullName,
                user.Email,
                user.UserName
            };
            
        }
    }
}
