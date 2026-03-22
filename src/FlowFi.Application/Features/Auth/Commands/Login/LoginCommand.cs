using FlowFi.Application.Common.Interfaces;
using FlowFi.Application.Features.Auth.Commands.Register;
using FlowFi.Domain.Common;
using FlowFi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<RegisterResponse>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<RegisterResponse>>
{
    private readonly IAppDbContext _db;
    private readonly IPasswordService _passwords;
    private readonly ITokenService _tokens;

    public LoginCommandHandler(IAppDbContext db, IPasswordService passwords, ITokenService tokens)
        => (_db, _passwords, _tokens) = (db, passwords, tokens);

    public async Task<Result<RegisterResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), ct);

        if (user is null || !_passwords.Verify(request.Password, user.PasswordHash))
            return Result<RegisterResponse>.Unauthorized("Invalid credentials");

        var (raw, hash) = _tokens.GenerateRefreshToken();
        var refreshToken = RefreshToken.Create(user.Id, hash, DateTime.UtcNow.AddSeconds(2592000));
        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync(ct);

        return Result<RegisterResponse>.Success(new RegisterResponse(
            new UserDto(user.Id, user.Email, user.FullName, user.Currency, user.Plan.ToString()),
            _tokens.GenerateAccessToken(user), raw, 900
        ));
    }
}
