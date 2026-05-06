using BrainDump.Application.Features.Move;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrainDump.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/move")]
public class MoveController : ControllerBase
{
    private readonly IMediator _mediator;

    public MoveController(IMediator mediator) => _mediator = mediator;

    public record MoveRequest(MoveKind Kind, int Id, int? TargetParentId, int Position);

    [HttpPost]
    public async Task<IActionResult> Move([FromBody] MoveRequest body, CancellationToken ct)
    {
        await _mediator.Send(new MoveNode(body.Kind, body.Id, body.TargetParentId, body.Position), ct);
        return NoContent();
    }
}
