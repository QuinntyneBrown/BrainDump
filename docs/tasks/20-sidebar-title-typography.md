# 20 — Tighten sidebar title weight + letter spacing

**Audit reference:** §8.1
**Design node:** `bWjsN` — "Notes" headline. `$font-display`, `$size-headline-sm`, `$weight-semibold`, `letterSpacing: -0.5`.

## Goal
Match the sidebar header typography to the design — semibold (not bold), with −0.5 px letter spacing.

## Files to touch
- `frontend/projects/components/src/lib/sidebar/sidebar.scss`

## Implementation

```scss
.bd-sidebar__title {
  font-family: var(--bd-font-display);
  font-size: var(--bd-size-headline-sm);
  font-weight: var(--bd-weight-semibold);  // was: var(--bd-weight-bold)
  letter-spacing: -0.5px;                  // new
  color: var(--bd-color-text-primary);
  margin: 0;
}
```

## Acceptance criteria
- "Notes" header in the sidebar renders in semibold (e.g. 600), not bold (700).
- Glyphs are slightly tightened (−0.5 px tracking) — visible at headline size.
- No regression for any other consumer of `.bd-sidebar__title` (only used by `BdSidebar`).
