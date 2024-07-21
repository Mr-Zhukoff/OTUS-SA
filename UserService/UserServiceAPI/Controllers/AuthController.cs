using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading;
using UserServiceAPI.Data;
using UserServiceAPI.Logic;
using UserServiceAPI.Models;

namespace UserServiceAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IConfiguration _config;
        private readonly string _pepper;
        private readonly UsersDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public AuthController(IConfiguration config, ILogger<UsersController> logger, UsersDbContext context)
        {
            _config = config;
            _logger = logger;
            _context = context;
            _pepper = _config["Auth:Pepper"];
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            //your logic for login process
            //If login usrename and password are correct then proceed to generate token


            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var Sectoken = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              null,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);

            return Ok(token);
        }
        [HttpPost]
        public async Task<ActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            var user = new User
            {
                Email = registerRequest.Email,
                PasswordSalt = PasswordHasher.GenerateSalt()
            };
            user.PasswordHash = PasswordHasher.ComputeHash(registerRequest.Password, user.PasswordSalt, _pepper);
            var result = _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(result);
        }
    }
}
