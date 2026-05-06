# UI Tasks — Home Page Audit Remediation

Each markdown file in this folder describes a single discrete UI change derived from `docs/ui-audits/home-page-audit.md`. Pick one, complete it end-to-end, mark its checkbox in the audit, then move to the next.

**Reference design:** `docs/user-interface-designs/design.pen` → frame `XL - Desktop - Editor` (id `A40kPP`).

**Standing rules for all tasks:**

- The Pencil MCP server is the authoritative way to read the design (`mcp__pencil__batch_get` + `mcp__pencil__get_screenshot`). Don't `Read` `.pen` files directly.
- Follow the existing component structure: shared components live in `frontend/projects/components/src/lib/<name>/<name>.{ts,html,scss}` and are re-exported from `frontend/projects/components/src/public-api.ts`.
- Use design tokens (`--bd-color-*`, `--bd-space-*`, `--bd-size-*`, `--bd-weight-*`, `--bd-radius-*`, `--bd-font-*`) — never hardcode hex/px values that map to a token.
- After a change, verify in the browser at http://localhost:4200/home (the `web` container hot-reloads on save) and run `npm run lint` if available.
- For any change touching a shared component, also update its e2e harness (`frontend/e2e/pages/home.page.ts`) if a `data-testid` is added or removed.

## Task index

01. [Add hamburger button to side rail](01-add-hamburger-button.md) ✅
02. [Show nav-item labels under icons in rail mode](02-show-nav-item-labels.md) ✅
03. [Change rail active highlight from circle to rounded square](03-rail-active-indicator-shape.md) ✅
04. [Add avatar at bottom of side rail](04-add-rail-avatar.md) ✅
05. [Widen rail to 80 px and grow item height](05-widen-rail.md) ✅
06. [Replace sidebar `+` icon button with "+ New" tonal button](06-sidebar-new-button.md) ✅
07. [Add search field below sidebar header](07-sidebar-search-field.md) ✅
08. [Add filter chips row](08-sidebar-filter-chips.md) ✅
09. [Enrich note item with icon, tags, and timestamp](09-note-item-structure.md) ✅
10. [Toolbar left: document icon + facts badge + edited caption](10-toolbar-left.md) ✅
11. [Toolbar right: visibility/history/share + Save tonal button](11-toolbar-right.md) ✅
12. [Toolbar divider + remove fill](12-toolbar-divider.md) ✅
13. [Editor body padding 48/80 and line gap 8](13-editor-body-spacing.md)
14. [Style heading markdown lines](14-heading-line-styling.md)
15. [Outline panel (right column)](15-outline-panel.md)
16. [Backlinks panel](16-backlinks-panel.md)
17. [Status bar footer](17-status-bar.md)
18. [Remove the FAB](18-remove-fab.md)
19. [Fix editor-body background token](19-editor-body-background.md)
20. [Tighten sidebar title weight + letter spacing](20-sidebar-title-typography.md)
