# 03 — Rail active indicator: rounded square, not circle

**Audit reference:** §1.3
**Design nodes:** active item `pI3ZX` — `cornerRadius: $radius-xl` on a fill `$color-primary-container`. The pill wraps only the **icon row**, not the label.

## Goal
Replace the current circular `border-radius: 50%` highlight with the M3 navigation-rail "indicator" pill — a rounded rectangle (~32 px tall, 56 px wide) that sits behind the icon. The label remains *outside* the pill.

## Design intent
- Indicator is a fixed-size pill (the design uses cornerRadius `$radius-xl` ≈ 16 px; height matches the icon row at ~32 px; width ~56 px).
- Indicator fill: `$color-primary-container`.
- Icon color inside the pill: `$color-primary`.
- Hover over inactive item shows a subtle state layer using the same shape.

## Files to touch
- `frontend/projects/components/src/lib/nav-item/nav-item.html`
- `frontend/projects/components/src/lib/nav-item/nav-item.scss`

## Implementation

Wrap the icon in an indicator element so the pill can be styled independently of the host. In `nav-item.html`:

```html
<span class="bd-nav-item__indicator">
  <mat-icon class="bd-nav-item__icon">{{ icon() }}</mat-icon>
</span>
<span class="bd-nav-item__label">{{ label() }}</span>
```

In `nav-item.scss`, add styling for the indicator and remove `border-radius: 50%` from the host:

```scss
:host([data-rail]) {
  // (see task 02 — vertical layout, no host border-radius)
  background: transparent;

  .bd-nav-item__indicator {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 56px;
    height: 32px;
    border-radius: var(--bd-radius-xl);
    transition: background-color 120ms ease;
  }

  &:hover .bd-nav-item__indicator {
    background: var(--bd-color-surface-2);
  }
}

:host([data-rail][data-active='true']) {
  background: transparent;

  .bd-nav-item__indicator {
    background: var(--bd-color-primary-container);
  }

  .bd-nav-item__icon {
    color: var(--bd-color-primary);
  }
}
```

Also delete the existing `:host([data-active='true']) { background: ... }` rule's effect inside rail mode — that styling is for the non-rail (sidebar/list) variant and should remain there.

## Acceptance criteria
- Active rail item shows a rounded-rectangle pill (≈ 56×32) behind the icon, in primary-container color.
- The pill does **not** wrap the label.
- Inactive items have no background; on hover, a subtle state layer appears in the same pill shape.
- The non-rail variant of `bd-nav-item` (used elsewhere if any) is unaffected.

## Dependencies
- Pairs with task 02 (labels) and task 05 (rail width). All three share rail-mode CSS.
