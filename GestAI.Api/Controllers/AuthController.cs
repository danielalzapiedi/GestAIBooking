using GestAI.Application.Login;
using GestAI.Domain.Entities;
using GestAI.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GestAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController(UserManager<User> userManager, ITokenService tokens) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null) return Unauthorized();
        if (!user.IsActive) return Unauthorized(new { message = "Usuario inactivo." });
        if (!await userManager.CheckPasswordAsync(user, request.Password)) return Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        var expires = DateTime.UtcNow.AddMinutes(120);
        user.LastLoginAtUtc = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        var token = tokens.Create(user.Id, user.Email!, user.Nombre, user.Apellido, roles, DateTime.UtcNow, expires);
        return Ok(new LoginResponse(token, expires));
    }
}
