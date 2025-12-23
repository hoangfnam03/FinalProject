using Application.Common.Models;

namespace Application.Common.Interfaces
{
    public interface IQuestionVectorService
    {
        IReadOnlyList<QuestionChunk> ChunkQuestion(string text);
        Task<IReadOnlyList<float>> EmbedAsync(string text, CancellationToken cancellationToken);
        Task IndexQuestionAsync(Guid questionId, string title, string body, CancellationToken cancellationToken);
    }
}
