using Engine.Helpers;
using Engine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.User;
using WebApp.Controllers.Base;

namespace WebApp.Controllers.Auth;
[Route("api/[controller]")]
[ApiController]
public class SignUpController : ApiController
{
    private readonly IConfiguration _config;
    private readonly UserService _userService;
    public SignUpController(UserService userService, IConfiguration config)
    {
        // Configuração do JWT e Dependencia do UserService
        _config = config;
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> SignUp([FromBody] CreateUserDto createUserDto)
    {
        if (createUserDto == null)
            return Error(400, "Dados do usuário devem ser preenchidos.");

        if (_userService.GetUserByEmailAsync(createUserDto.Email).Result != null)
            return Error(400, "Usuario com email ja existente!");

        var userViewModel = await _userService.SignUp(createUserDto);

        if (userViewModel.HasErrors)
            return Error(400, string.Join(",", userViewModel.Errors));

        var jwtToken = JWTHelper.GenerateToken(userViewModel.Id, userViewModel.Email, _config);

        return Created("", new { accessToken = jwtToken });
    }
}
