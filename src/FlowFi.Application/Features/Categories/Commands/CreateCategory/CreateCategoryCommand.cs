using FlowFi.Application.Common.Interfaces;
using FlowFi.Application.Features.Transactions.Commands.CreateTransaction;
using FlowFi.Domain.Common;
using FlowFi.Domain.Entities;
using FlowFi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(Guid UserId, string Name, string? Icon, string? Color) : IRequest<Result<CategoryDto>>;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly IAppDbContext _db;

    public CreateCategoryCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
        if (user is null) return Result<CategoryDto>.Unauthorized();

        if (user.Plan == UserPlan.Free)
        {
            var count = await _db.Categories.CountAsync(c => c.UserId == request.UserId, ct);
            if (count >= 3)
            {
                return Result<CategoryDto>.Failure("Free plan allows maximum 3 custom categories", "PLAN_REQUIRED");
            }
        }

        var category = Category.CreateUserDefined(request.UserId, request.Name, request.Icon, request.Color);
        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);

        return Result<CategoryDto>.Success(new CategoryDto(category.Id, category.Name, category.Icon, category.Color));
    }
}
