// QnA_BE/Controllers/MeController.cs
using Application.Common.Models;
using Application.Members.Commands.UpdateMyProfile;
using Application.Members.Commands.UploadAvatar;
using Application.Members.DTOs;
using Application.Members.Queries.GetMyProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace QnA_BE.Controllers
{
    [ApiController]
    [Route("api/v1/me")]
    [Authorize]
    public class MeController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MeController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<MeDto>> Get(CancellationToken ct)
            => Ok(await _mediator.Send(new GetMyProfileQuery(), ct));

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<MeDto>> Update([FromBody] MeUpdateRequest req, CancellationToken ct)
            => Ok(await _mediator.Send(new UpdateMyProfileCommand(req), ct));

        // ✅ Thêm Consumes để Swagger sinh đúng schema
        [HttpPut("avatar")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(6_000_000)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<FileDto>> UploadAvatar(IFormFile file, CancellationToken ct)
            => Ok(await _mediator.Send(new UploadAvatarCommand(file), ct));
    }
}
