using BrainDump.Application.Features.Folders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrainDump.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/folders")]
public class FoldersController : ControllerBase
{
    private readonly IMediator _mediator;

    public FoldersController(IMediator mediator) => _mediator = mediator;

    public record CreateFolderRequest(int? ParentId, string Title, int Position);
    public record UpdateFolderRequest(string Title, int Position);

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateFolderRequest body, CancellationToken ct)
    {
        var id = await _mediator.Send(new CreateFolder(body.ParentId, body.Title, body.Position), ct);
        return CreatedAtAction(nameof(Create), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFolderRequest body, CancellationToken ct)
    {
        await _mediator.Send(new UpdateFolder(id, body.Title, body.Position), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteFolder(id), ct);
        return NoContent();
    }
}
