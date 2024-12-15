using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using UserServiceAPI.Data;
using UserServiceAPI.Models;

namespace UserServiceAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersDbContext _context;
        private readonly ILogger<UsersController> _logger;
        private readonly IConfiguration _configuration;

        public UsersController(ILogger<UsersController> logger, UsersDbContext context, IConfiguration config)
        {
            _logger = logger;
            _context = context;
            _configuration = config;

            try
            {
                if (!_context.Database.CanConnect())
                {
                    _context.Database.EnsureDeleted();
                    _context.Database.EnsureCreated();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UsersController initialization error!");
            }

        }

        // healthcheck
        [AllowAnonymous]
        [HttpGet("health")]
        public ActionResult Health()
        {
            try
            {
                _logger.LogInformation($"Health status requested {Environment.MachineName}");

                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

                return Ok(new
                {
                    status = "OK",
                    version = fvi.FileVersion,
                    machinename = Environment.MachineName,
                    osversion = Environment.OSVersion.VersionString,
                    processid = Environment.ProcessId,
                    timestamp = DateTime.Now,
                    database = _context.Database.CanConnect(),
                    pgconnstr = Environment.GetEnvironmentVariable("PG_CONNECTION_STRING")
                });
            }

            catch (Exception ex)
            {
                return Ok(new
                {
                    status = "BAD",
                    machinename = Environment.MachineName,
                    osversion = Environment.OSVersion.VersionString,
                    processid = Environment.ProcessId,
                    message = ex.Message
                });
            }
            finally
            {
                _context.Database.CloseConnection();
            }
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            int userId = GetUserId();

            if (id != userId)
                return BadRequest("Modifying another user is not allowed!");

            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            return user;
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(PostUser), new { id = user.Id });
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            try
            {
                int userId = GetUserId();

                if (id != userId)
                    return BadRequest("Modifying another user is not allowed!");

                var curuser = await _context.Users.FindAsync(id);
                if (curuser == null)
                    return NotFound();

                curuser.FirstName = user.FirstName;
                curuser.LastName = user.LastName;
                curuser.MiddleName = user.MiddleName;
                curuser.Email = user.Email;

                _context.Entry(curuser).State = EntityState.Modified;


                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                    return NotFound();
                else
                    throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PutUser Error!");
                return StatusCode(500, ex.Message);
            }

            return NoContent();
        }

        // PATCH: api/users/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchUser(int id, User user)
        {
            try
            {
                int userId = GetUserId();

                if (id != userId)
                    return BadRequest("Modifying another user is not allowed!");

                var curuser = await _context.Users.FindAsync(id);
                if (curuser == null)
                    return NotFound();

                curuser.FirstName = user.FirstName;
                curuser.LastName = user.LastName;
                curuser.MiddleName = user.MiddleName;
                curuser.Email = user.Email;

                _context.Entry(curuser).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                    return NotFound();
                else
                    throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PatchUser Error!");
                return StatusCode(500, ex.Message);
            }

            return NoContent();
        }

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound();

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteUser Error!");
                return StatusCode(500, ex.Message);
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        private int GetUserId()
        {
            var jwt = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", string.Empty);
            var tokenData = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
            var userIdValue = tokenData.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            int userId = 0;
            int.TryParse(userIdValue, out userId);
            return userId;
        }
    }
}
