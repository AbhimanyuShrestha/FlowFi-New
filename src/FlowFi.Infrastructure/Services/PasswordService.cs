using FlowFi.Application.Common.Interfaces;

namespace FlowFi.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private const int WorkFactor = 12;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
