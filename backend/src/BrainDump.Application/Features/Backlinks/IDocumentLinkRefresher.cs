namespace BrainDump.Application.Features.Backlinks;

public interface IDocumentLinkRefresher
{
    /// <summary>
    /// Re-extracts wiki-style references from every section title and fact
    /// text in <paramref name="sourceDocumentId"/> and replaces the
    /// corresponding rows in document_link.
    /// </summary>
    Task RefreshAsync(int sourceDocumentId, CancellationToken ct = default);
}
