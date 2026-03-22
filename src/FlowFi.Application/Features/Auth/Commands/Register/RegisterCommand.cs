using FlowFi.Application.Common.Interfaces;
using FlowFi.Domain.Common;
using FlowFi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string? FullName,
    string Currency = "USD"
) : IRequest<Result<RegisterResponse>>;

public record RegisterResponse(
    UserDto User,
    string AccessToken,
    string RefreshToken,
    int ExpiresIn
);

public record UserDto(Guid Id, string Email, string? FullName, string Currency, string Plan);

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordService _passwords;
    private readonly ITokenService _tokens;

    public RegisterCommandHandler(IAppDbContext db, IPasswordService passwords, ITokenService tokens)
        => (_db, _passwords, _tokens) = (db, passwords, tokens);

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken ct)
    {
        var exists = await _db.Users.AnyAsync(u => u.Email == request.Email.ToLowerInvariant(), ct);
        if (exists) return Result<RegisterResponse>.Conflict("Email already registered");

        var user = User.Create(
            request.Email,
            _passwords.Hash(request.Password),
            request.FullName,
            request.Currency
        );

        _db.Users.Add(user);

        var (raw, hash) = _tokens.GenerateRefreshToken();
        var refreshToken = RefreshToken.Create(user.Id, hash, DateTime.UtcNow.AddSeconds(2592000));
        _db.RefreshTokens.Add(refreshToken);

        await _db.SaveChangesAsync(ct);

        var accessToken = _tokens.GenerateAccessToken(user);

        return Result<RegisterResponse>.Success(new RegisterResponse(
            new UserDto(user.Id, user.Email, user.FullName, user.Currency, user.Plan.ToString()),
            accessToken, raw, 900
        ));
    }
}
