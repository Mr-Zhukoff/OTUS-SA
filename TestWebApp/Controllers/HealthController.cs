using Microsoft.AspNetCore.Mvc;

namespace TestWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HealthController(ILogger<HealthController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public ActionResult Get()
        {
            
            _logger.LogInformation($"Health status requested {Environment.MachineName}");
            return Ok(new { 
                status = "OK",
                //host = _httpContextAccessor?.HttpContext?.Request.Host.Value,
                //method = _httpContextAccessor?.HttpContext?.Request.Method,
                host = Environment.MachineName,
                os = Environment.OSVersion.VersionString,
                //processid = Environment.ProcessId
            });
        }
    }
}