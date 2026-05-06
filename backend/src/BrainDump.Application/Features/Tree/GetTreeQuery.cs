using BrainDump.Application.DTOs;
using MediatR;

namespace BrainDump.Application.Features.Tree;

public record GetTreeQuery() : IRequest<TreeDto>;
