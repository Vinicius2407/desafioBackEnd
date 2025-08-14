using Engine.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Auth;
[Route("api/[controller]")]
[ApiController]
public class SignUpController : ControllerBase
{
    public SignUpController() { }

    [HttpPost]
    public IActionResult SignUp([FromBody] Models.DTOs.User.CreateUserDto createUserDto)
    {
        if (createUserDto == null)
            return BadRequest("Dados do usuário não podem ser nulos.");

        var userViewModel = UserService.SignUp(createUserDto);

        if (userViewModel.HasErrors)
            return BadRequest(new { userViewModel.Errors });

        var jtwToken = Engine.Helpers.GenerateToken(userViewModel);

        return ;
    }
}
