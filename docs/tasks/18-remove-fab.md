# 18 — Remove the floating action button ✅ Completed

**Audit reference:** §6.2

## Goal
The floating "+" action button on the editor is not present in the design and conflicts with the new status bar. Remove it. Section creation is performed via the sidebar's "+ New" button (task 06).

## Files to touch
- `frontend/projects/brain-dump/src/app/home/home.html`
- `frontend/projects/brain-dump/src/app/home/home.scss`

## Implementation

In `home.html`, delete the FAB block:

```html
<!-- DELETE: -->
<div class="home-shell__fab">
  <bd-fab
    icon="add"
    ariaLabel="Add section"
    (click)="onAddRootSection()"
    data-testid="fab-add-section"
  />
</div>
```

In `home.scss`, delete `.home-shell__fab { … }`.

In `home.ts`, drop the `BdFab` import from the component imports list.

If e2e tests reference `data-testid="fab-add-section"`, redirect them to `data-testid="add-section"` (the sidebar header button — task 06).

## Acceptance criteria
- No FAB rendered on the home page.
- `BdFab` is no longer imported by `Home`.
- E2e specs that previously clicked the FAB now click the sidebar's "+ New" button.

## Dependencies
- Sidebar "+ New" button (task 06) must exist before users lose the FAB; coordinate sequencing.
