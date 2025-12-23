using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QnA_BE.Controllers.Admin
{
    [ApiController]
    [Route("api/v1/admin")]
    [Authorize] // phải đăng nhập
    public abstract class AdminBaseController : ControllerBase
    {
    }
}
