using Microsoft.AspNetCore.Mvc;

namespace nfse_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] dynamic body)
    {
        return Ok(new { user = body?.user ?? "admin" });
    }
}