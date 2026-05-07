using BrainDump.Application.DTOs;
using BrainDump.Application.Features.Recents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrainDump.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/recent")]
public class RecentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RecentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RecentDocumentDto>>> Get(
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var rows = await _mediator.Send(new GetRecentDocuments(limit), ct);
        return Ok(rows);
    }
}
