# 01 — Add hamburger button to side rail ✅ Completed

**Audit reference:** §1.1
**Design node:** `HcPVc` (instance of `oaqpE` Component/Button/Icon, icon `menu`) inside `t2jT2` (nav rail)

## Goal
Render a hamburger (`menu`) icon button at the very top of the left navigation rail, above the nav items.

## Design intent
The first interactive element in the rail. Sits inside an 8 px top spacer. Uses the same `oaqpE` icon-button component as other icon buttons in the design system; icon color `$color-text-primary`.

## Files to touch
- `frontend/projects/brain-dump/src/app/home/home.html` — add the button as the first child of `<bd-side-rail>`.

## Implementation

In `home.html`, replace:

```html
<bd-side-rail>
  <bd-nav-item label="Notes" icon="description" [active]="true" />
  …
</bd-side-rail>
```

with:

```html
<bd-side-rail>
  <bd-icon-button
    class="home-shell__rail-menu"
    icon="menu"
    ariaLabel="Toggle navigation"
    (click)="onToggleNav()"
    data-testid="rail-menu"
  />
  <bd-nav-item label="Notes" icon="description" [active]="true" />
  …
</bd-side-rail>
```

In `home.ts`, add a no-op handler for now (the toggle behavior isn't part of this task):

```ts
protected onToggleNav(): void {
  // TODO: wire to layout state once a nav-collapsed signal exists
}
```

## Acceptance criteria
- A `menu` icon button is visible at the top of the rail, before the four nav items.
- Tab order: hamburger → Notes → Tags → Recent → Settings → avatar (once §1.4 lands).
- The button is keyboard-focusable and has an accessible name "Toggle navigation".
- Clicking the button does not throw; the click handler is wired but is a no-op.

## Out of scope
- Actually collapsing/expanding the nav. That's a separate behavior that can be added once a layout signal is introduced.
