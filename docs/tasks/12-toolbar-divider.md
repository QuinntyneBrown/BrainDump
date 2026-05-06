# 12 — Toolbar divider + remove fill ✅ Completed

**Audit reference:** §3.3
**Design nodes:** `uTvUy` (toolbar — no fill) followed by `hJQor` (1 px rectangle, fill `$color-border-subtle`).

## Goal
The toolbar should sit transparently over the editor surface. A 1 px divider line follows it, separating the toolbar from the editor body.

## Files to touch
- `frontend/projects/components/src/lib/top-app-bar/top-app-bar.scss`
- `frontend/projects/brain-dump/src/app/home/home.scss`

## Implementation

### Strip toolbar fill
In `top-app-bar.scss`:

```scss
.bd-top-app-bar {
  background: transparent;
  border-bottom: 1px solid var(--bd-color-border-subtle);
  // override Material defaults that paint a surface tone:
  --mat-toolbar-container-background-color: transparent;
  --mat-toolbar-container-text-color: var(--bd-color-text-primary);
}
```

If `--bd-color-border-subtle` doesn't exist yet, add it to the design-tokens stylesheet (probably `frontend/projects/components/src/lib/styles/_tokens.scss` or wherever `--bd-color-border` lives) by mapping it to the design's `$color-border-subtle` value (a low-opacity neutral — verify in `mcp__pencil__get_variables`).

### Verify toolbar height/padding
The design's toolbar is 64 px tall with `padding: [24, 0]` (24 horizontal, 0 vertical). Confirm against current `mat-toolbar` defaults; override if needed.

## Acceptance criteria
- Toolbar background is transparent (shows the editor surface beneath).
- A 1 px line in `--bd-color-border-subtle` separates the toolbar from the editor body.
- No double-divider artifacts when paired with the existing `mat-toolbar` styles.
