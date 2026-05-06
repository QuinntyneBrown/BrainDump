using BrainDump.Application.DTOs;
using BrainDump.Application.Features.Workspace;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrainDump.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/workspace")]
public class WorkspaceController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkspaceController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<WorkspaceDto>> Get(CancellationToken ct)
    {
        var ws = await _mediator.Send(new GetWorkspace(), ct);
        return Ok(ws);
    }
}
