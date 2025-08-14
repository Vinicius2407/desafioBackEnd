using Engine.Services;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.User;
using System.Reflection.Metadata.Ecma335;
using Engine.Helpers;
using WebApp.Controllers.Base;
using Microsoft.AspNetCore.Authorization;

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
        var user = await _userService.GetUserByEmailAsync(loginUserDto.Email);

        if (user == null) return Error(401, "Usuario ou Senha incorretos.");

        if (!Engine.Helpers.PasswordHelper.VerifyPassword(hashedPassword: user.Password, password: loginUserDto.Password))
            return Error(401, "Usuario ou Senha incorretos.");

        var jwtToken = JWTHelper.GenerateToken(user.Id, user.Email, _config);

        return Ok(new
        {
            accessToken = jwtToken,
        });
    }
}
