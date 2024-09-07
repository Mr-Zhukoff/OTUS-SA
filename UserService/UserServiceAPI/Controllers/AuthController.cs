using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
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
        public IActionResult Login([FromBody] LoginForm loginForm)
        {
            try
            {
                _logger.LogInformation($"Login form requested {loginForm.ToString()}");
                var existingUser = _context.Users.FirstOrDefault(x => x.Email.ToLower() == loginForm.Email.ToLower());
                if (existingUser == null)
                {
                    _logger.LogWarning($"User ({loginForm.Email}) not found!");
                    return BadRequest($"Пользователь с email '{loginForm.Email}' не существует!");
                }

                if (!CheckPassword(loginForm.Password, existingUser))
                {
                    return BadRequest($"Пароль не правильный!");
                }

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var Sectoken = new JwtSecurityToken(_config["Jwt:Issuer"],
                  _config["Jwt:Issuer"],
                  claims: new List<Claim>
                        {
                        new Claim(ClaimTypes.Email, existingUser.Email),
                        new Claim("id", existingUser.Id.ToString()),
                        },
                  notBefore: DateTime.UtcNow,
                  expires: DateTime.Now.AddMinutes(120),
                  signingCredentials: credentials);

                var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);

                return Ok(token);
            }
            catch (Exception ex) {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Register([FromBody] RegisterForm registerForm)
        {
            try
            {
                _logger.LogInformation($"Register form requested {registerForm.ToString()}");
                var existingUser = _context.Users.FirstOrDefault(x => x.Email.ToLower() == registerForm.Email.ToLower());
                if (existingUser != null)
                {
                    _logger.LogWarning($"User already exist!");
                    return BadRequest($"Пользователь с email {registerForm.Email} уже зарегистрирован!");
                }

                var user = new User
                {
                    FirstName = registerForm.FirstName,
                    LastName = registerForm.LastName,
                    MiddleName = registerForm.MiddleName,
                    Email = registerForm.Email,
                    PasswordSalt = PasswordHasher.GenerateSalt()
                };
                user.PasswordHash = PasswordHasher.ComputeHash(registerForm.Password, user.PasswordSalt, _pepper);
                var result = _context.Users.Add(user);
                var result2 = await _context.SaveChangesAsync();

                return Ok(result.ToString());
            }
            catch (Exception ex) {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Logout()
        {
            _logger.LogInformation($"Logout user");
            Response.Headers.Remove("Authorization");

            return Ok();
        }

        private bool CheckPassword(string pwd, User user)
        {
            var passwordHash = PasswordHasher.ComputeHash(pwd, user.PasswordSalt, _pepper);
            return (user.PasswordHash == passwordHash);
        }
    }
}
