using Engine.Services;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Bet;
using WebApp.Controllers.Base;

namespace WebApp.Controllers;
[Route("api/[controller]")]
[ApiController]
public class BetController : ProtectedController
{
    private readonly BetService _betService;
    public BetController(BetService betService)
    {
        _betService = betService;
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult<BetViewModel>> PlaceBet([FromBody] CreateBetDto createBetDto)
    {
        var errors = ValidadeDataAnnotations<CreateBetDto>(createBetDto);

        if (errors.Any())
            return Error(400, string.Join(", ", errors));

        var betViewModel = await _betService.PlaceBetAsync(createBetDto);

        if (betViewModel.HasErrors)
            return Error(400, string.Join(", ", betViewModel.Errors));

        betViewModel = await _betService.ProcessBetResultAsync(betViewModel.Id);

        if (betViewModel.HasErrors)
            return Error(400, string.Join(", ", betViewModel.Errors));

        return betViewModel;
    }
}
