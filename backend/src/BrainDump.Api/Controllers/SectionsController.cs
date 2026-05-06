using BrainDump.Application.Features.Sections;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrainDump.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/sections")]
public class SectionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SectionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public record CreateSectionRequest(int DocumentId, int? ParentId, string Title, int Position);
    public record UpdateSectionRequest(int? ParentId, string Title, int Position);

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateSectionRequest body, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateSection(body.DocumentId, body.ParentId, body.Title, body.Position), cancellationToken);
        return Ok(id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSectionRequest body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateSection(id, body.ParentId, body.Title, body.Position), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteSection(id), cancellationToken);
        return NoContent();
    }
}
