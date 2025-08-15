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
    private readonly WalletService _walletService;
    public BetController(BetService betService, WalletService walletService)
    {
        _betService = betService;
        _walletService = walletService;
    }

    [HttpGet]
    [Route("{betId}")]
    public async Task<ActionResult<BetViewModel>> GetById([FromRoute] long betId)
    {
        var betViewModel = await _betService.GetFullBetByIdAsync(betId);

        if (betViewModel.HasErrors)
            return Error(400, string.Join(", ", betViewModel.Errors));

        return betViewModel;
    }

    [HttpGet]
    [Route("byUser/{userId}")]
    public ActionResult<PaginationResponse<BetViewModel>> GetAll([FromRoute] long userId, [FromQuery] PaginationRequest paginationRequest)
    {
        var itemListPagined = _betService.GetBetsPaginedByUserId(userId, paginationRequest.Page, paginationRequest.ItemsPerPage);
        var response = new PaginationResponse<BetViewModel>(itemListPagined, itemListPagined.ToList());
        return response;
    }

    [HttpGet]
    [Route("all")]
    public ActionResult<PaginationResponse<BetViewModel>> GetAll([FromQuery] PaginationRequest paginationRequest)
    {
        var itemListPagined = _betService.GetBetsPaginedByUserId(null, paginationRequest.Page, paginationRequest.ItemsPerPage);
        var response = new PaginationResponse<BetViewModel>(itemListPagined, itemListPagined.ToList());
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

        if (createBetDto.AutoPlayOnCreate)
            betViewModel = await _betService.ProcessBetResultAsync((long)betViewModel.Id!);

        if (betViewModel.HasErrors)
            return Error(400, string.Join(", ", betViewModel.Errors));

        return betViewModel;
    }

    [HttpGet]
    [Route("{betId}/execute")]
    public async Task<ActionResult<BetViewModel>> ProcessBetResultAsync([FromRoute] long betId)
    {
        var betViewModel = await _betService.GetBetByIdAsync(betId);
        if (betViewModel == null)
            return Error(400, "Aposta nao encontrada");

        if (betViewModel.Status != Models.Helpers.Enumerators.BetStatus.PENDING)
            return Error(400, "Aposta não esta pendente!");

        betViewModel = await _betService.ProcessBetResultAsync(betId);

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

        // Logica simples
        if (betViewModel.Status != Models.Helpers.Enumerators.BetStatus.PENDING)
            return Error(400, "Cancelamento negado, aposta não esta mas pendente.");

        if (wallet.BalanceBlocked < betViewModel.Amount)
            return Error(400, "Cancelamento negado, valores de bloqueio para estorno menor que valor da aposta.");

        betViewModel = await _betService.UpdateBetStatusAsync((long)betViewModel.Id!, Models.Helpers.Enumerators.BetStatus.CANCELLED);

        if (betViewModel.HasErrors)
            return Error(400, string.Join(", ", betViewModel.Errors));

        return NoContent();
    }
}
