# 05 — Widen rail to 80 px and grow item height to 64 px ✅ Completed

**Audit reference:** §1.5, §1.6
**Design nodes:** `t2jT2.width = 80`, `padding: [0, 12]`, `gap: $space-2`. Each nav item: `height: 64`, `width: 64`.

## Goal
Resize the side rail and its items to match the M3 navigation-rail spec from the design.

## Files to touch
- `frontend/projects/components/src/lib/side-rail/side-rail.scss`
- `frontend/projects/components/src/lib/nav-item/nav-item.scss`

## Implementation

`side-rail.scss`:

```scss
:host {
  display: flex;
  flex-direction: column;
  align-items: stretch;
  gap: var(--bd-space-2);          // was --bd-space-1
  width: 80px;                     // was 64px
  flex-shrink: 0;
  padding: var(--bd-space-3) var(--bd-space-3); // 12 12 — close to design [0,12]
  background: var(--bd-color-surface);
  border-right: 1px solid var(--bd-color-border);
  height: 100%;
  box-sizing: border-box;
  overflow-y: auto;
  overflow-x: hidden;              // matches sidebar fix in commit ee4ac42's spirit
}
```

`nav-item.scss` rail-mode block (in conjunction with tasks 02 and 03):

```scss
:host([data-rail]) {
  flex-direction: column;
  width: 64px;     // matches the 64-wide square in the design
  height: 64px;    // was 48
  padding: 0;
  gap: 4px;
  justify-content: center;
  align-self: center;
}
```

## Acceptance criteria
- Side rail measures 80 px wide.
- Nav items are 64×64, comfortably fitting a 32 px indicator pill above the label.
- No horizontal scrollbar in the rail.

## Dependencies
- Bundle with tasks 02 + 03 + 04 — same files, same visual region.
