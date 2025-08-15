using Engine.Helpers;
using Engine.Services;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.User;
using WebApp.Controllers.Base;

namespace WebApp.Controllers.Auth;
[Route("api/[controller]")]
[ApiController]
public class SignInController : ApiController
{
    private readonly IConfiguration _config;
    private readonly UserService _userService;
    public SignInController(UserService userService, IConfiguration config)
    {
        // Configuração do JWT e Dependencia do UserService
        _config = config;
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(LoginUserDto loginUserDto)
    {
        var errors = ValidadeDataAnnotations<LoginUserDto>(loginUserDto);

        if (errors.Any())
            return Error(400, string.Join(",", errors));

        var user = await _userService.GetUserByEmailAsync(loginUserDto.Email);

        if (user == null) return Error(401, "Usuario ou Senha incorretos.");

        if (!Engine.Helpers.PasswordHelper.VerifyPassword(hashedPassword: user.Password, password: loginUserDto.Password))
            return Error(401, "Usuario ou Senha incorretos.");

        var jwtToken = JWTHelper.GenerateToken(user.Id, user.Email, _config);

        var userFull = await _userService.GetFullUserByIdAsync(user.Id);

        return Ok(new
        {
            accessToken = jwtToken,
            user = userFull
        });
    }
}
