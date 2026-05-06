# 02 — Show nav-item labels under icons in rail mode

**Audit reference:** §1.2
**Design nodes:** `pI3ZX` (Notes), `rDt7t` (Tags), `aLyQ2` (Recent), `uQ0Ue` (Settings) — all 64×64 vertical frames inside `t2jT2`.

## Goal
In the side rail, each nav item must show its label *below* the icon (M3 navigation rail convention). Today the label is hidden via `display: none` when the item is in rail mode.

## Design intent
- Frame: 64×64, vertical layout, `justifyContent: center`, `alignItems: center`, gap 4.
- Icon: 24×24, `Material Symbols Rounded`.
- Label: `$font-body`, `$size-label-sm`, weight `$weight-medium` (active) / `normal` (inactive).
- Active item label color: `$color-primary`; inactive: `$color-text-secondary`.

## Files to touch
- `frontend/projects/components/src/lib/nav-item/nav-item.scss`
- (Possibly) `frontend/projects/components/src/lib/nav-item/nav-item.html` — no change expected if styling alone is enough.

## Implementation

In `nav-item.scss` rail-mode block, remove the label hide and switch to a stacked layout:

```scss
:host([data-rail]) {
  flex-direction: column;
  width: 64px;
  height: 64px;
  padding: 0;
  gap: 4px;
  justify-content: center;
  align-self: center;
  border-radius: 0; // see task 03 for the indicator pill

  .bd-nav-item__label {
    display: block;
    font-size: var(--bd-size-label-sm);
    font-weight: var(--bd-weight-medium);
    color: var(--bd-color-text-secondary);
    line-height: 1;
  }
}

:host([data-rail][data-active='true']) .bd-nav-item__label {
  color: var(--bd-color-primary);
}
```

Drop the previous `width: 48px; height: 48px; border-radius: 50%;` rules — those are replaced by tasks 03 and 05.

## Acceptance criteria
- All four nav items display their text label ("Notes", "Tags", "Recent", "Settings") under the icon.
- Active item label is rendered in primary color; others in text-secondary.
- No horizontal overflow inside the 80 px rail (after task 05 lands).

## Dependencies
- Best landed together with task 03 (indicator shape) and task 05 (rail width / item height) — they touch the same selector.
