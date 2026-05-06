# 07 — Add search field below sidebar header ✅ Completed

**Audit reference:** §2.2
**Design node:** `hxSEx` — instance of `pJc4a` (Component/TextField), `width: fill_container`, placed between the header (`q96orU`) and the chips row (`qAuJQ`).

## Goal
Render a full-width search input below the "Notes / + New" header in the sidebar.

## Design intent
- Full-width text field, height ~40 px, leading `search` icon, placeholder "Search notes…".
- Filled or outlined per the design system — match `pJc4a` (the screenshot shows a subtle surface fill with rounded corners).

## Files to touch
- `frontend/projects/brain-dump/src/app/home/home.html`
- (Optionally) a `BdSearchField` component, or reuse Material's `<mat-form-field appearance="outline">` with a `matPrefix` icon.

## Implementation

Use Angular Material with a thin wrapper. In `home.html`, between the projected header actions and the note items:

```html
<mat-form-field
  class="home-shell__search"
  appearance="outline"
  subscriptSizing="dynamic"
>
  <mat-icon matPrefix>search</mat-icon>
  <input
    matInput
    type="search"
    placeholder="Search notes…"
    [(ngModel)]="searchQuery"
    aria-label="Search notes"
    data-testid="sidebar-search"
  />
</mat-form-field>
```

In `home.ts`:

```ts
import { FormsModule } from '@angular/forms';
import { MatFormField, MatInput, MatPrefix } from '@angular/material/...';
…
imports: […, FormsModule, MatFormField, MatInput, MatPrefix, MatIcon],
…
protected searchQuery = signal('');
```

In `home.scss`:

```scss
.home-shell__search {
  width: 100%;

  ::ng-deep .mat-mdc-text-field-wrapper {
    border-radius: var(--bd-radius-full);
    background: var(--bd-color-surface);
  }
}
```

(Filtering behavior is out of scope — wire `searchQuery()` into `rootSections` later.)

## Acceptance criteria
- A search field is visible below the "Notes" header, full-width within the sidebar's padding.
- Placeholder reads "Search notes…".
- Field is keyboard-accessible (tab order between header and note items).
- Typing into the field updates `searchQuery` (no filtering yet).
