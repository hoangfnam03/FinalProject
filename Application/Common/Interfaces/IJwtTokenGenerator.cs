using System.Collections.Generic;

namespace Application.Common.Interfaces
{
    public interface IJwtTokenGenerator
    {
        (string AccessToken, int ExpiresInSeconds) Generate(long userId, string email, IEnumerable<(string type, string value)>? extraClaims = null);
    }
}
