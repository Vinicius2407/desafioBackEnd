using Engine.Services;
using Microsoft.AspNetCore.Http;
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
}
