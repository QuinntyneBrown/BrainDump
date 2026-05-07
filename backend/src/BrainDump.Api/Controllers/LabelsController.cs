using BrainDump.Application.Features.Labels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrainDump.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/labels")]
public class LabelsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LabelsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<string>>> Get(CancellationToken ct)
    {
        var rows = await _mediator.Send(new GetWorkspaceLabels(), ct);
        return Ok(rows);
    }
}
