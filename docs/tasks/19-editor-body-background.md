# 19 — Fix editor-body background token ✅ Completed

**Audit reference:** §7.2
**Design intent:** code area (`S1v8d`) and editor body (`Tby2u`) sit on `$color-surface`, not the page-level `$color-bg`.

## Goal
Switch `.home-shell__editor-body` background from `--bd-color-bg` to `--bd-color-surface` so the editor content visually sits on the same surface tone as the rest of the document chrome.

## Files to touch
- `frontend/projects/brain-dump/src/app/home/home.scss`

## Implementation

```scss
.home-shell__editor-body {
  …
  background: var(--bd-color-surface);  // was: var(--bd-color-bg)
}
```

Sanity-check the rest of the layout:

- `.home-shell` background should remain `--bd-color-bg` (the page).
- `bd-side-rail` host: `--bd-color-surface` (matches design).
- `bd-sidebar` host: `--bd-color-surface-2` (matches design's note-list fill `$color-surface-2`).

If the sidebar background is currently `--bd-color-surface`, fix it too — the design uses `surface-2` for the note list column.

## Acceptance criteria
- Editor body background matches the toolbar / outline panel surface.
- Sidebar (note list) is one tone darker (`surface-2`), creating the M3 layered look.
- No visual seams between toolbar and editor body.
