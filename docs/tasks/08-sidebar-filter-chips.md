# 08 — Add filter chips row to sidebar ✅ Completed

**Audit reference:** §2.3
**Design nodes:** `qAuJQ` (`dtChips`) containing `DnPbG` (All — selected, primary container), `x8wCuz` (#facts), `TSOf9` (#wip). Each is an instance of `sDkRY` (Component/Chip).

## Goal
Render a horizontal row of three filter chips beneath the sidebar search field: `All`, `#facts`, `#wip`. The first chip ("All") is the selected state.

## Design intent
- Chips are pill-shaped, height ~28 px, border 1 px, padding `[6, 12]`.
- Selected chip: `$color-primary-container` fill, no stroke (`stroke.thickness: 0`).
- Unselected chip: transparent fill, `$color-border` 1 px stroke, text-secondary label.
- Row gap: `$space-2` (8 px).

## Files to touch
- New component: `frontend/projects/components/src/lib/chip/chip.{ts,html,scss}` (if not already present).
- `frontend/projects/brain-dump/src/app/home/home.html`
- `frontend/projects/components/src/public-api.ts`

## Implementation

Sketch `BdChip`:

```ts
@Component({
  selector: 'bd-chip',
  template: `<ng-content />`,
  host: {
    '[attr.data-selected]': 'selected()',
    '[attr.role]': '"button"',
    '[attr.tabindex]': '0',
  },
  styleUrl: './chip.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdChip {
  readonly selected = input(false);
}
```

`chip.scss`:

```scss
:host {
  display: inline-flex;
  align-items: center;
  height: 28px;
  padding: 0 var(--bd-space-3);
  border-radius: var(--bd-radius-full);
  border: 1px solid var(--bd-color-border);
  background: transparent;
  color: var(--bd-color-text-secondary);
  font-family: var(--bd-font-body);
  font-size: var(--bd-size-label-md);
  font-weight: var(--bd-weight-medium);
  cursor: pointer;
  transition: background-color 120ms ease, color 120ms ease;
}

:host([data-selected='true']) {
  background: var(--bd-color-primary-container);
  border-color: transparent;
  color: var(--bd-color-primary);
}
```

In `home.html`, after the search field:

```html
<div class="home-shell__chips">
  <bd-chip [selected]="filter() === 'all'" (click)="filter.set('all')">All</bd-chip>
  <bd-chip [selected]="filter() === 'facts'" (click)="filter.set('facts')">#facts</bd-chip>
  <bd-chip [selected]="filter() === 'wip'" (click)="filter.set('wip')">#wip</bd-chip>
</div>
```

In `home.ts`:

```ts
protected readonly filter = signal<'all' | 'facts' | 'wip'>('all');
```

In `home.scss`:

```scss
.home-shell__chips {
  display: flex;
  gap: var(--bd-space-2);
  flex-wrap: nowrap;
  margin-bottom: var(--bd-space-3);
}
```

## Acceptance criteria
- Three chips render in order: All, #facts, #wip.
- "All" is initially selected (primary-container fill).
- Clicking a different chip updates the selection visually.
- No filtering behavior is wired yet (out of scope).
