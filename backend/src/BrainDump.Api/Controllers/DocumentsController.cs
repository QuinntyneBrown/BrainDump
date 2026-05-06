using BrainDump.Application.DTOs;
using BrainDump.Application.Features.Documents;
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
}
