using FlowFi.API.Extensions;
using FlowFi.Application.Features.Dashboard.Queries.GetDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowFi.API.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ISender _sender;
    public DashboardController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return (await _sender.Send(new GetDashboardQuery(userId), ct)).ToActionResult(this);
    }
}
