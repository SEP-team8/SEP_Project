using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var result = await _authService.Register(request);

        return result.Status switch
        {
            ResponseStatus.Created => Ok(new { token = result.Data?.Token, user = result.Data?.User }),
            ResponseStatus.BadRequest => BadRequest(new { error = result.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Message })
        };
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
    {
        var result = await _authService.Login(request);

        return result.Status switch
        {
            ResponseStatus.OK => Ok(new { token = result.Data?.Token, user = result.Data?.User }),
            ResponseStatus.Unauthorized => Unauthorized(new { error = result.Message }),
            ResponseStatus.NotFound => NotFound(new { error = result.Message }),
            ResponseStatus.BadRequest => BadRequest(new { error = result.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Message })
        };
    }
}
