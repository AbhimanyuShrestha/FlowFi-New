using FlowFi.Application.Common.Interfaces;
using FlowFi.Application.Features.Transactions.Commands.CreateTransaction;
using FlowFi.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.Application.Features.Categories.Queries.ListCategories;

public record ListCategoriesQuery(Guid UserId) : IRequest<Result<List<CategoryDto>>>;

public class ListCategoriesQueryHandler : IRequestHandler<ListCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly IAppDbContext _db;

    public ListCategoriesQueryHandler(IAppDbContext db) => _db = db;

    public async Task<Result<List<CategoryDto>>> Handle(ListCategoriesQuery request, CancellationToken ct)
    {
        var categories = await _db.Categories
            .Where(c => c.UserId == request.UserId || c.UserId == null)
            .OrderByDescending(c => c.IsDefault)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Icon, c.Color))
            .ToListAsync(ct);

        return Result<List<CategoryDto>>.Success(categories);
    }
}
