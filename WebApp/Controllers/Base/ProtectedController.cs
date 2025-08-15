using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;

namespace WebApp.Controllers.Base;
[Authorize]
public class ProtectedController : ApiController
{
    private long? _userId;
    protected long UserIdAutenticado
    {
        get
        {
            if (_userId == null)
            {
                var authorizationHeader = HttpContext.Request.Headers["Authorization"];
                var token = authorizationHeader.ToString().Substring("Bearer ".Length).Trim();
                _userId = Convert.ToInt64(Engine.Helpers.JWTHelper.DeserializeToken(token));
            }
            return _userId.Value;
        }
    }
}
