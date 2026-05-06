# Detailed Designs — Index

| # | Feature | Status | Description |
|---|---------|--------|-------------|
| 01 | [DB-Backed User Authentication](01-db-backed-user-auth/README.md) | Implemented | Adds a `user` table and routes both local PKCE sign-in and production Entra ID validation through a DB-backed user record. |
| 02 | [Documents and Folders](02-documents-and-folders/README.md) | Draft | Foundational slice for the multi-document workspace: `folder`, `document` tables; document-scoped tree read; move endpoint. |
| 03 | [Multi-Document Editing](03-multi-document-editing/README.md) | Draft | Per-user open-tabs persistence and side-by-side split editor. |
| 04 | [Recent Activity](04-recent-activity/README.md) | Draft | Per-user `user_document_view` log with view-record + recents-list endpoints. |
| 05 | [Document Labels](05-document-labels/README.md) | Draft | Workspace-scoped tags with chip UI and label-based filtering. |
| 06 | [Cross-Document Backlinks](06-backlinks/README.md) | Draft | `[[wiki-link]]` extraction into `document_link`; right-rail backlinks panel. |
| 07 | [Global Search](07-global-search/README.md) | Draft | Cmd+K palette over SQL Server full-text search across titles, fact text, and labels. |
| 08 | [Page Templates](08-page-templates/README.md) | Draft | Reusable starter trees stored in `template_*` tables; deep-copied into new documents. |

## Vertical-slice ordering

Slice 02 is foundational — slices 03–08 each depend on the document/folder model in 02 but are **independent of each other** and can be implemented in any order after 02 lands. Each slice follows ATDD: the Playwright POM specs listed in each design are written first, drive the implementation end-to-end (DB schema → MediatR handler → Web API → Angular UI), and are green when the slice ships.
