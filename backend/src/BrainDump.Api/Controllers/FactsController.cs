using BrainDump.Application.Features.Facts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrainDump.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/facts")]
public class FactsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FactsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public record CreateFactRequest(int SectionId, string Text, int Position);
    public record UpdateFactRequest(int SectionId, string Text, int Position);

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateFactRequest body, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateFact(body.SectionId, body.Text, body.Position), cancellationToken);
        return Ok(id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFactRequest body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateFact(id, body.SectionId, body.Text, body.Position), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteFact(id), cancellationToken);
        return NoContent();
    }
}
