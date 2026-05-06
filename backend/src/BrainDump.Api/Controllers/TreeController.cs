using BrainDump.Application.DTOs;
using BrainDump.Application.Features.Tree;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrainDump.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tree")]
public class TreeController : ControllerBase
{
    private readonly IMediator _mediator;

    public TreeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<TreeDto>> GetTree(CancellationToken cancellationToken)
    {
        var tree = await _mediator.Send(new GetTreeQuery(), cancellationToken);
        return Ok(tree);
    }
}
