# 13 — Editor body padding 48/80 and line gap 8 ✅ Completed

**Audit reference:** §4.1, §4.2
**Design node:** `Tby2u` ("editor body") — `padding: [48, 80]`, `gap: 8`.

## Goal
Increase the editor body's padding and inter-line spacing to match the design.

## Files to touch
- `frontend/projects/brain-dump/src/app/home/home.scss`

## Implementation

```scss
.home-shell__editor-body {
  flex: 1;
  overflow-y: auto;
  padding: 48px 80px;     // was: var(--bd-space-6) var(--bd-space-4)
  display: flex;
  flex-direction: column;
  gap: 8px;               // was: 2px
  background: var(--bd-color-surface);  // see task 19
}
```

Prefer tokens if equivalents exist:

- 48 px → `--bd-space-12` (if defined)
- 80 px → `--bd-space-20` (if defined)
- 8 px → `--bd-space-2`

Check `frontend/projects/components/src/lib/styles/_tokens.scss` (or equivalent) before hardcoding pixels.

## Acceptance criteria
- Editor body padding is 48 px (top/bottom) by 80 px (left/right).
- Vertical spacing between rendered lines is 8 px.
- Horizontal scrollbar does not appear at typical desktop widths.

## Dependencies
- Pairs naturally with task 19 (background token).
