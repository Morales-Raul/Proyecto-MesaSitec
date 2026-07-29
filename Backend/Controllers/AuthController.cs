using Microsoft.AspNetCore.Mvc;
using Api.Auth;

namespace Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.Login(request);
        if (response == null)
            return Unauthorized(new
            {
                type = "https://mesasitec.local/errores/no-autenticado",
                title = "Credenciales inválidas",
                status = 401,
                detail = "El email o la contraseña son incorrectos.",
                codigo = "NO_AUTENTICADO"
            });

        return Ok(response);
    }
}