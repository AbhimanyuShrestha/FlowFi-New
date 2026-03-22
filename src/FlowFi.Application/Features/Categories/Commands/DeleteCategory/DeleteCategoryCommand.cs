using FlowFi.Application.Common.Interfaces;
using FlowFi.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.Application.Features.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(Guid UserId, Guid CategoryId) : IRequest<Result>;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly IAppDbContext _db;

    public DeleteCategoryCommandHandler(IAppDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.UserId == request.UserId, ct);

        if (category is null) return Result.NotFound("Category");

        if (category.IsDefault)
        {
            return Result.Failure("Cannot delete system categories", "FORBIDDEN");
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
