namespace BrainDump.Application.DTOs;

public record FolderDto(int Id, int? ParentId, string Title, int Position);
