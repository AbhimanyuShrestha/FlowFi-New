using FlowFi.Application.Common.Interfaces;
using FlowFi.Application.Features.Transactions.Commands.CreateTransaction;
using FlowFi.Domain.Enums;
using FlowFi.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FlowFi.Application.Tests.Features.Transactions;

public class CreateTransactionCommandTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly SqliteConnection _connection;

    public CreateTransactionCommandTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _cacheMock = new Mock<ICacheService>();
    }

    [Fact]
    public async Task CreateTransaction_WithNewIdempotencyKey_ReturnsIsNewTrue()
    {
        // Arrange
        var user = FlowFi.Domain.Entities.User.Create("a@b.com", "hash", "User", "USD");
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var handler = new CreateTransactionCommandHandler(_db, _cacheMock.Object);
        var command = new CreateTransactionCommand(
            user.Id, 100, TransactionType.Expense, "Test", null, null, null, "key-1"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsNew);
        Assert.Equal(100, result.Value.Amount);
    }

    [Fact]
    public async Task CreateTransaction_WithDuplicateIdempotencyKey_ReturnsExistingTransaction()
    {
        // Arrange
        var user = FlowFi.Domain.Entities.User.Create("b@c.com", "hash", "User", "USD");
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var handler = new CreateTransactionCommandHandler(_db, _cacheMock.Object);
        var command = new CreateTransactionCommand(
            user.Id, 100, TransactionType.Expense, "Test", null, null, null, "dup-key"
        );

        // First call
        var result1 = await handler.Handle(command, CancellationToken.None);
        Assert.True(result1.Value!.IsNew);

        // Act - Second call
        var result2 = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result2.IsSuccess);
        Assert.False(result2.Value!.IsNew);
        Assert.Equal(result1.Value.Id, result2.Value.Id);
        
        var count = await _db.Transactions.CountAsync();
        Assert.Equal(1, count);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
