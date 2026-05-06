namespace BrainDump.Application.DTOs;

public record WorkspaceDto(
    IReadOnlyList<FolderDto> Folders,
    IReadOnlyList<DocumentDto> Documents);
