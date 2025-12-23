using MediatR;

namespace Application.Admin.Documents.Commands
{
    public record DeleteDocumentCommand(long DocumentId) : IRequest;
}
