using Application.Admin.Documents.Commands;
using Application.Admin.Documents.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace QnA_BE.Controllers.Admin
{
    public class AdminDocumentsController : AdminBaseController
    {
        private readonly IMediator _mediator;

        public AdminDocumentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("documents/upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentForm form, CancellationToken ct)
        {

            if (form.File == null || form.File.Length == 0)
                return BadRequest("File is required.");

            var dto = await _mediator.Send(new UploadDocumentCommand(form.File), ct);
            return Ok(dto);
        }

        public class UploadDocumentForm
        {
            public IFormFile File { get; set; } = default!;
        }

        [HttpGet("documents")]
        public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListDocumentsQuery(page, pageSize), ct);
            return Ok(result);
        }

        [HttpDelete("documents/{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            await _mediator.Send(new DeleteDocumentCommand(id), ct);
            return NoContent();
        }
    }
}
