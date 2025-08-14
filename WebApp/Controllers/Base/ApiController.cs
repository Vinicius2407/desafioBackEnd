using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Base;

[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase
{
    protected IActionResult Error(int code, string message)
    {
        var errorResponse = new ResponseError(code, message);
        return BadRequest(errorResponse);
    }
}

public class ResponseError
{
    public ResponseError(int code, string message)
    {
        StatusCode = code;
        Error = message;
    }
    public string Error { get; set; } = string.Empty;
    public int StatusCode { get; set; }
}
