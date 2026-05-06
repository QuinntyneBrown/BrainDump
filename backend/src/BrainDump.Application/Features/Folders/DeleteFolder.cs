using BrainDump.Application.Exceptions;
using BrainDump.Application.Interfaces;
using MediatR;

namespace BrainDump.Application.Features.Folders;

public record DeleteFolder(int Id) : IRequest<Unit>;

public class DeleteFolderHandler : IRequestHandler<DeleteFolder, Unit>
{
    private readonly IFolderRepository _folders;

    public DeleteFolderHandler(IFolderRepository folders) => _folders = folders;

    public async Task<Unit> Handle(DeleteFolder request, CancellationToken cancellationToken)
    {
        if (await _folders.FindAsync(request.Id, cancellationToken) is null)
            throw new NotFoundException($"Folder {request.Id} not found");

        await _folders.DeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
