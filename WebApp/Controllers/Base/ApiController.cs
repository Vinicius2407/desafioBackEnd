using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Controllers.Base;

[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase
{
    protected ActionResult Error(int code, string message)
    {
        var errorResponse = new ResponseError(code, message);
        return BadRequest(errorResponse);
    }

    protected List<ValidationResult> ValidadeDataAnnotations<T>(T validatingDto)
    {
        var validationContext = new ValidationContext(validatingDto!);
        var validationResults = new List<ValidationResult>();
        _ = Validator.TryValidateObject(validatingDto!, validationContext, validationResults, true);

        return validationResults;
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
