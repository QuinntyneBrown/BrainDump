using BrainDump.Application.DTOs;
using BrainDump.Application.Features.Documents;
using BrainDump.Application.Features.Labels;
using BrainDump.Application.Features.Recents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrainDump.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator) => _mediator = mediator;

    public record CreateDocumentRequest(int? FolderId, string Title, int Position);
    public record UpdateDocumentRequest(string Title, int Position);

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateDocumentRequest body, CancellationToken ct)
    {
        var id = await _mediator.Send(new CreateDocument(body.FolderId, body.Title, body.Position), ct);
        return CreatedAtAction(nameof(Create), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDocumentRequest body, CancellationToken ct)
    {
        await _mediator.Send(new UpdateDocument(id, body.Title, body.Position), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteDocument(id), ct);
        return NoContent();
    }

    [HttpGet("{id:int}/tree")]
    public async Task<ActionResult<TreeDto>> GetTree(int id, CancellationToken ct)
    {
        var tree = await _mediator.Send(new GetDocumentTree(id), ct);
        return Ok(tree);
    }

    [HttpPost("{id:int}/view")]
    public async Task<IActionResult> RecordView(int id, CancellationToken ct)
    {
        await _mediator.Send(new RecordView(id), ct);
        return NoContent();
    }

    public record SetLabelsRequest(IReadOnlyList<string> Labels);

    [HttpPut("{id:int}/labels")]
    public async Task<IActionResult> SetLabels(int id, [FromBody] SetLabelsRequest body, CancellationToken ct)
    {
        await _mediator.Send(new SetDocumentLabels(id, body.Labels ?? Array.Empty<string>()), ct);
        return NoContent();
    }
}
