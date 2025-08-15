using Engine.Services;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.User;
using Models.DTOs.Wallet;
using WebApp.Controllers.Base;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApp.Controllers;
[Route("api/[controller]")]
public class UserController : ProtectedController
{
    private readonly UserService _userService;
    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Route("{userId}")]
    public async Task<ActionResult<FullUserView>> GetUserById([FromRoute] long userId)
    {
        var fullUserView = await _userService.GetFullUserByIdAsync(userId);

        if (fullUserView.HasErrors)
            return Error(400, string.Join(", ", fullUserView.Errors));

        return fullUserView;
    }

    [HttpGet]
    [Route("{userId}/wallet")]
    public async Task<ActionResult<WalletViewModel>> GetWallet([FromRoute] long userId)
    {
        var walletView = await _userService.GetWalletAsync(userId);
        if (walletView.HasErrors)
            return Error(400, string.Join(", ", walletView.Errors));
        return walletView;
    }
}
