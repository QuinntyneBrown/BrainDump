using BrainDump.Application.Exceptions;
using BrainDump.Application.Interfaces;
using MediatR;

namespace BrainDump.Application.Features.Folders;

public record CreateFolder(int? ParentId, string Title, int Position) : IRequest<int>;

public class CreateFolderHandler : IRequestHandler<CreateFolder, int>
{
    private readonly IFolderRepository _folders;

    public CreateFolderHandler(IFolderRepository folders) => _folders = folders;

    public async Task<int> Handle(CreateFolder request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Title must not be empty");
        if (request.ParentId is int pid && await _folders.FindAsync(pid, cancellationToken) is null)
            throw new NotFoundException($"Parent folder {pid} not found");

        var folder = await _folders.CreateAsync(request.ParentId, request.Title.Trim(), request.Position, cancellationToken);
        return folder.Id;
    }
}
