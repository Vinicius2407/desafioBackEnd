using Engine.Services;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Bet;
using WebApp.Controllers.Base;
using X.PagedList;

namespace WebApp.Controllers;
[Route("api/[controller]")]
[ApiController]
public class BetController : ProtectedController
{
    private readonly BetService _betService;
    private readonly WalletService _walletService;
    public BetController(BetService betService, WalletService walletService)
    {
        _betService = betService;
        _walletService = walletService;
    }

    [HttpGet]
    public ActionResult<PaginationResponse<BetViewModel>> GetAll([FromQuery] PaginationRequest paginationRequest)
    {
        var itemListPagined = _betService.GetBetsPaginedByUserId(AuthenticatedUserId, paginationRequest.Page, paginationRequest.ItemsPerPage);

        var response = new PaginationResponse<BetViewModel>
        {
            Page = itemListPagined.PageNumber,
            ItemsPerPage = itemListPagined.PageSize,
            TotalItems = itemListPagined.TotalItemCount,
            TotalPages = itemListPagined.PageCount,
            Items = itemListPagined.ToList()
        };

        return response;
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult<BetViewModel>> PlaceBet([FromBody] CreateBetDto createBetDto)
    {
        var errors = ValidadeDataAnnotations<CreateBetDto>(createBetDto);

        if (errors.Any())
            return Error(400, string.Join(", ", errors));

        createBetDto.UserId = AuthenticatedUserId;

        var betViewModel = await _betService.PlaceBetAsync(createBetDto);

        if (betViewModel.HasErrors)
            return Error(400, string.Join(", ", betViewModel.Errors));

        betViewModel = await _betService.ProcessBetResultAsync(betViewModel.Id);

        if (betViewModel.HasErrors)
            return Error(400, string.Join(", ", betViewModel.Errors));

        return betViewModel;
    }

    [HttpPut]
    [Route("{betId}/cancel")]
    public async Task<ActionResult> CancelBet(long betId)
    {
        if (betId == 0)
            return Error(400, "Id inválido");

        var betViewModel = await _betService.GetBetByIdAsync(betId);

        if (betViewModel.HasErrors)
            return Error(400, string.Join(", ", betViewModel.Errors));

        var wallet = await _walletService.GetWalletByUserIdAsync(AuthenticatedUserId);

        // Perguntar sobre a validação
        if (wallet.BalanceAvailable < betViewModel.Amount)
            return Error(400, "Cancelamento negado, valor na carteira menor que o valor da aposta");

        betViewModel = await _betService.UpdateBetStatusAsync(betViewModel.Id, Models.Helpers.Enumerators.BetStatus.CANCELLED);

        if (betViewModel.HasErrors)
            return Error(400, string.Join(", ", betViewModel.Errors));

        return NoContent();
    }
}
