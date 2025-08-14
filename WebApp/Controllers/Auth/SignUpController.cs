using Engine.Services;
using Engine.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Auth;
[Route("api/[controller]")]
[ApiController]
public class SignUpController : ControllerBase
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
    public async Task<IActionResult> SignUp([FromBody] Models.DTOs.User.CreateUserDto createUserDto)
    {
        if (createUserDto == null)
            return BadRequest("Dados do usuário não podem ser nulos.");

        var userViewModel = await _userService.SignUp(createUserDto);

        if (userViewModel.HasErrors)
            return BadRequest(new { userViewModel.Errors });

        var jwtToken = JWTHelper.GenerateToken(userViewModel.Id, userViewModel.Email, _config);

        return Created("", new { accessToken = jwtToken });
    }
}
