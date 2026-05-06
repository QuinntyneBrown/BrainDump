using BrainDump.Application.DTOs;
using BrainDump.Application.Features.Reorder;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrainDump.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reorder")]
public class ReorderController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReorderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public record ReorderRequest(List<ReorderItem> Items);

    [HttpPost]
    public async Task<IActionResult> Reorder([FromBody] ReorderRequest body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReorderSiblings(body.Items), cancellationToken);
        return NoContent();
    }
}
