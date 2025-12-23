using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Vectorization
{
    public class QuestionVectorService : IQuestionVectorService
    {
        private readonly HttpClient _httpClient;
        private readonly QdrantSettings _qdrantSettings;
        private readonly EmbeddingSettings _embeddingSettings;
        private readonly VectorChunkingSettings _chunkingSettings;

        public QuestionVectorService(
            HttpClient httpClient,
            IOptions<QdrantSettings> qdrantOptions,
            IOptions<EmbeddingSettings> embeddingOptions,
            IOptions<VectorChunkingSettings> chunkingOptions)
        {
            _httpClient = httpClient;
            _qdrantSettings = qdrantOptions.Value;
            _embeddingSettings = embeddingOptions.Value;
            _chunkingSettings = chunkingOptions.Value;
        }

        public IReadOnlyList<QuestionChunk> ChunkQuestion(string text)
        {
            var normalized = (text ?? string.Empty).Replace("\r\n", " \n ").Replace("\n", " \n ");
            var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 0)
                return Array.Empty<QuestionChunk>();

            var maxWords = Math.Max(10, _chunkingSettings.MaxWords);
            var overlap = Math.Clamp(_chunkingSettings.OverlapWords, 0, maxWords - 1);
            var chunks = new List<QuestionChunk>();

            for (var start = 0; start < words.Length; start += (maxWords - overlap))
            {
                var current = words.Skip(start).Take(maxWords).ToArray();
                if (current.Length == 0)
                    break;

                var chunkText = string.Join(' ', current).Trim();
                if (!string.IsNullOrWhiteSpace(chunkText))
                    chunks.Add(new QuestionChunk(chunks.Count, chunkText));

                if (start + maxWords >= words.Length)
                    break;
            }

            return chunks;
        }

        public async Task<IReadOnlyList<float>> EmbedAsync(string text, CancellationToken cancellationToken)
        {
            if (!_qdrantSettings.Enabled)
                return Array.Empty<float>();

            if (string.IsNullOrWhiteSpace(_embeddingSettings.ApiKey))
                throw new InvalidOperationException("Embedding API key is missing. Set Embeddings:ApiKey before indexing.");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/embeddings")
            {
                Content = JsonContent.Create(new
                {
                    input = text,
                    model = _embeddingSettings.Model
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _embeddingSettings.ApiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Embedding response is empty.");

            var vector = payload.Data.FirstOrDefault()?.Embedding
                ?? throw new InvalidOperationException("Embedding response does not contain data.");

            return vector;
        }

        public async Task IndexQuestionAsync(Guid questionId, string title, string body, CancellationToken cancellationToken)
        {
            if (!_qdrantSettings.Enabled)
                return;

            await EnsureCollectionExistsAsync(cancellationToken);

            var chunks = ChunkQuestion($"{title}\n\n{body}");
            if (chunks.Count == 0)
                return;

            var points = new List<object>();
            foreach (var chunk in chunks)
            {
                var embedding = await EmbedAsync(chunk.Content, cancellationToken);
                points.Add(new
                {
                    id = $"{questionId:N}-{chunk.Index}",
                    vector = embedding,
                    payload = new
                    {
                        questionId,
                        title,
                        chunk = chunk.Content,
                        chunkIndex = chunk.Index
                    }
                });
            }

            var upsertRequest = new HttpRequestMessage(
                HttpMethod.Put,
                $"{_qdrantSettings.Endpoint.TrimEnd('/')}/collections/{_qdrantSettings.CollectionName}/points")
            {
                Content = JsonContent.Create(new
                {
                    points
                })
            };
            ApplyQdrantAuth(upsertRequest);

            var upsertResponse = await _httpClient.SendAsync(upsertRequest, cancellationToken);
            upsertResponse.EnsureSuccessStatusCode();
        }

        private void ApplyQdrantAuth(HttpRequestMessage message)
        {
            if (!string.IsNullOrWhiteSpace(_qdrantSettings.ApiKey))
                message.Headers.Add("api-key", _qdrantSettings.ApiKey);
        }

        private async Task EnsureCollectionExistsAsync(CancellationToken cancellationToken)
        {
            var createRequest = new HttpRequestMessage(
                HttpMethod.Put,
                $"{_qdrantSettings.Endpoint.TrimEnd('/')}/collections/{_qdrantSettings.CollectionName}")
            {
                Content = JsonContent.Create(new
                {
                    vectors = new
                    {
                        size = _qdrantSettings.VectorSize,
                        distance = _qdrantSettings.Distance
                    }
                })
            };
            ApplyQdrantAuth(createRequest);

            var response = await _httpClient.SendAsync(createRequest, cancellationToken);
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.Conflict)
            {
                response.EnsureSuccessStatusCode();
            }
        }

        private class EmbeddingResponse
        {
            [JsonPropertyName("data")]
            public List<EmbeddingItem> Data { get; set; } = new();
        }

        private class EmbeddingItem
        {
            [JsonPropertyName("embedding")]
            public List<float> Embedding { get; set; } = new();
        }
    }

    public class QdrantSettings
    {
        public bool Enabled { get; set; }
        public string Endpoint { get; set; } = "http://localhost:6333";
        public string CollectionName { get; set; } = "qna_questions";
        public string Distance { get; set; } = "Cosine";
        public int VectorSize { get; set; } = 1536;
        public string? ApiKey { get; set; }
    }

    public class EmbeddingSettings
    {
        public string Provider { get; set; } = "openai";
        public string Model { get; set; } = "text-embedding-3-small";
        public string? ApiKey { get; set; }
    }

    public class VectorChunkingSettings
    {
        public int MaxWords { get; set; } = 200;
        public int OverlapWords { get; set; } = 40;
    }
}
