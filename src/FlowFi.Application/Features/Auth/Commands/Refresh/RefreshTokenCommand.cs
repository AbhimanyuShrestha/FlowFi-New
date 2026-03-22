using FlowFi.Application.Common.Interfaces;
using FlowFi.Domain.Common;
using FlowFi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace FlowFi.Application.Features.Auth.Commands.Refresh;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<RefreshTokenResponse>>;

public record RefreshTokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IAppDbContext _db;
    private readonly ITokenService _tokens;

    public RefreshTokenCommandHandler(IAppDbContext db, ITokenService tokens)
        => (_db, _tokens) = (db, tokens);

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.RefreshToken))).ToLowerInvariant();

        var token = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (token is null || !token.IsActive)
            return Result<RefreshTokenResponse>.Unauthorized("Invalid or expired refresh token");

        token.Revoke();

        var (newRaw, newHash) = _tokens.GenerateRefreshToken();
        var newToken = RefreshToken.Create(token.UserId, newHash, DateTime.UtcNow.AddSeconds(2592000));
        _db.RefreshTokens.Add(newToken);

        await _db.SaveChangesAsync(ct);

        return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse(
            _tokens.GenerateAccessToken(token.User), newRaw, 900
        ));
    }
}
