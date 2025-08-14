using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers.Base;
[Authorize]
public class ProtectedController : ApiController { }
