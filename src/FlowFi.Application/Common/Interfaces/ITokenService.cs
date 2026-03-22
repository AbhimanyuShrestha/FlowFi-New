using FlowFi.Domain.Entities;

namespace FlowFi.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    (string Raw, string Hash) GenerateRefreshToken();
}
