using Engine.Services;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.Transaction;
using WebApp.Controllers.Base;

namespace WebApp.Controllers;
[Route("api/[controller]")]
[ApiController]
public class TransactionController : ProtectedController
{
    private readonly TransactionService _transactionService;
    public TransactionController(TransactionService transactionService)
    {
        _transactionService = transactionService;
    }
    [HttpGet]
    public ActionResult<PaginationResponse<TransactionViewModel>> GetAll([FromQuery] PaginationRequest paginationRequest)
    {
        var itemListPagined = _transactionService.GetTransactionsPaginedByWalletId(AuthenticatedUserId, paginationRequest.Page, paginationRequest.ItemsPerPage);
        var response = new PaginationResponse<TransactionViewModel>(itemListPagined, itemListPagined.ToList());
        return response;
    }

    [HttpPost]
    [Route("movement")]
    public async Task<ActionResult<TransactionViewModel>> CreateMovementAsync([FromBody] CreateTransactionDto withdrawTransactionDto)
    {
        var errors = ValidadeDataAnnotations<CreateTransactionDto>(withdrawTransactionDto);

        if (errors.Any())
            return Error(400, string.Join(", ", errors));

        withdrawTransactionDto.UserId = AuthenticatedUserId;
        var transactionViewModel = await _transactionService.CreateMovementAsync(withdrawTransactionDto);

        if (transactionViewModel.HasErrors)
            return Error(400, string.Join(", ", transactionViewModel.Errors));

        return transactionViewModel;
    }
}
