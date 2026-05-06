namespace BrainDump.Application.DTOs;

public record TabPaneDto(IReadOnlyList<int> Tabs, int ActiveIndex);

public record TabStateDto(IReadOnlyList<TabPaneDto> Panes);
