namespace BrainDump.Application.DTOs;

public record SectionDto(int Id, int? ParentId, string Title, int Position);
