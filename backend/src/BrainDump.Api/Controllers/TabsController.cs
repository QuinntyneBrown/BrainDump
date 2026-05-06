using BrainDump.Application.DTOs;
using BrainDump.Application.Features.Tabs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrainDump.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tabs")]
public class TabsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TabsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<TabStateDto>> Get(CancellationToken ct)
    {
        var state = await _mediator.Send(new GetTabs(), ct);
        return Ok(state);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] TabStateDto state, CancellationToken ct)
    {
        await _mediator.Send(new PutTabs(state), ct);
        return NoContent();
    }
}
