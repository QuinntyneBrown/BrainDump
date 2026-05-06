using BrainDump.Application.Exceptions;
using BrainDump.Application.Interfaces;
using MediatR;

namespace BrainDump.Application.Features.Folders;

public record UpdateFolder(int Id, string Title, int Position) : IRequest<Unit>;

public class UpdateFolderHandler : IRequestHandler<UpdateFolder, Unit>
{
    private readonly IFolderRepository _folders;

    public UpdateFolderHandler(IFolderRepository folders) => _folders = folders;

    public async Task<Unit> Handle(UpdateFolder request, CancellationToken cancellationToken)
    {
        var folder = await _folders.FindAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Folder {request.Id} not found");
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Title must not be empty");

        folder.Title = request.Title.Trim();
        folder.Position = request.Position;
        await _folders.UpdateAsync(folder, cancellationToken);
        return Unit.Value;
    }
}
