namespace BrainDump.Application.DTOs;

public record DocumentDto(
    int Id,
    int? FolderId,
    string Title,
    int Position,
    DateTime CreatedAt,
    DateTime UpdatedAt);
