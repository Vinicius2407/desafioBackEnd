using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Controllers.Base;

[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase
{
    public class PaginationResponse<T>
    {
        public int Page { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public List<T> Items { get; set; } = new List<T>();
    }

    public class PaginationRequest
    {
        public int ItemsPerPage { get; set; } = 5;
        public int Page { get; set; } = 1;
    }

    protected ActionResult Error(int code, string message)
    {
        var errorResponse = new ResponseError(code, message);
        return BadRequest(errorResponse);
    }

    protected List<string> ValidadeDataAnnotations<T>(T validatingDto)
    {
        var validationContext = new ValidationContext(validatingDto!);
        var validationResults = new List<ValidationResult>();
        _ = Validator.TryValidateObject(validatingDto!, validationContext, validationResults, true);

        var errorsList = validationResults.Select(x => x.ErrorMessage).ToList();

        return errorsList;
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
