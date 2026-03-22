using FlowFi.API.Extensions;
using FlowFi.Application.Features.Categories.Commands.CreateCategory;
using FlowFi.Application.Features.Categories.Commands.DeleteCategory;
using FlowFi.Application.Features.Categories.Queries.ListCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowFi.API.Controllers;

[ApiController]
[Route("api/v1/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ISender _sender;
    public CategoriesController(ISender sender) => _sender = sender;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => (await _sender.Send(new ListCategoriesQuery(CurrentUserId), ct)).ToActionResult(this);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command, CancellationToken ct)
    {
        var cmd = command with { UserId = CurrentUserId };
        return (await _sender.Send(cmd, ct)).ToCreatedResult(this);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => (await _sender.Send(new DeleteCategoryCommand(CurrentUserId, id), ct)).ToActionResult(this);
}
