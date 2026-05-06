# 15 — Outline panel (right column) ✅ Completed

**Audit reference:** §5
**Design nodes:** `wAvSZ` (right rail container, 340 wide, padding 32, gap `$space-4`); `lgz1T` ("OUTLINE" label); `O0yO0R` (`dtOL`) → 4 row instances `LiLtv`, `K5d1OS`, `ieBbd`, `I7IrP`.

## Goal
Add a 340 px right column hosting the document outline. The outline lists the current document's headings; the active heading is highlighted.

## Design intent

**Section label** (`lgz1T`):
- Text: "OUTLINE", uppercase, `$font-body`, `$size-label-sm`, `$weight-semibold`, letter-spacing 1, fill `$color-text-tertiary`.

**Outline rows** (`LiLtv` etc.):
- Horizontal layout, gap 8, padding `[10, 16]` (or `[10, 8]` for the active row), `cornerRadius: $radius-md`.
- Leading 14×14 `tag` icon (Material Symbols Rounded).
- Label text, `$font-body`, `$size-body-md`.
- Active state: fill `$color-primary-container`, icon and text `$color-primary`, weight `$weight-medium`.
- Inactive: no fill; icon `$color-text-tertiary`; text `$color-text-secondary`, normal weight.

## Files to touch
- New component: `frontend/projects/components/src/lib/outline/outline.{ts,html,scss}`.
- `frontend/projects/components/src/public-api.ts` — export the component.
- `frontend/projects/brain-dump/src/app/home/home.html` — add the right column.
- `frontend/projects/brain-dump/src/app/home/home.scss` — sizing, hide/show breakpoints.
- `frontend/projects/brain-dump/src/app/home/home.ts` — derive outline entries from `lines()` (or `tree()`).

## Implementation

### `BdOutline`

```ts
export interface BdOutlineEntry {
  readonly id: string | number;
  readonly label: string;
  readonly level: 1 | 2 | 3;
}

@Component({
  selector: 'bd-outline',
  imports: [MatIcon],
  templateUrl: './outline.html',
  styleUrl: './outline.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdOutline {
  readonly entries = input.required<readonly BdOutlineEntry[]>();
  readonly activeId = input<string | number | null>(null);
  readonly entryClick = output<BdOutlineEntry>();
}
```

Template:

```html
<h3 class="bd-outline__heading">OUTLINE</h3>
<nav class="bd-outline__list">
  @for (e of entries(); track e.id) {
    <button
      type="button"
      class="bd-outline__row"
      [attr.data-active]="e.id === activeId()"
      [attr.data-level]="e.level"
      (click)="entryClick.emit(e)"
    >
      <mat-icon class="bd-outline__icon">tag</mat-icon>
      <span class="bd-outline__label">{{ e.label }}</span>
    </button>
  }
</nav>
```

Styles:

```scss
:host {
  display: flex;
  flex-direction: column;
  gap: var(--bd-space-2);
}

.bd-outline__heading {
  margin: 0;
  font-family: var(--bd-font-body);
  font-size: var(--bd-size-label-sm);
  font-weight: var(--bd-weight-semibold);
  letter-spacing: 1px;
  color: var(--bd-color-text-tertiary);
  text-transform: uppercase;
}

.bd-outline__list {
  display: flex;
  flex-direction: column;
  gap: var(--bd-space-2);
}

.bd-outline__row {
  all: unset;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  border-radius: var(--bd-radius-md);
  color: var(--bd-color-text-secondary);
  font-family: var(--bd-font-body);
  font-size: var(--bd-size-body-md);
}

.bd-outline__icon {
  width: 14px;
  height: 14px;
  font-size: 14px;
  color: var(--bd-color-text-tertiary);
}

.bd-outline__row[data-active='true'] {
  background: var(--bd-color-primary-container);
  color: var(--bd-color-primary);
  font-weight: var(--bd-weight-medium);

  .bd-outline__icon { color: var(--bd-color-primary); }
}

.bd-outline__row[data-level='2'] { padding-left: 24px; }
.bd-outline__row[data-level='3'] { padding-left: 32px; }
```

### Wire into Home

In `home.ts`:

```ts
protected readonly outlineEntries = computed<BdOutlineEntry[]>(() => {
  return this.lines()
    .filter(l => l.kind === 'section')
    .map(l => ({
      id: l.sectionId!,
      label: this.tree().sections.find(s => s.id === l.sectionId)?.title ?? '',
      level: ((l.depth ?? 0) + 1) as 1 | 2 | 3,
    }));
});

protected readonly activeOutlineId = signal<number | null>(null);
```

In `home.html` (after adding the column wrapper):

```html
<aside class="home-shell__right-rail">
  <bd-outline
    [entries]="outlineEntries()"
    [activeId]="activeOutlineId()"
    (entryClick)="onOutlineClick($event)"
  />
  <!-- Backlinks panel goes here (task 16) -->
</aside>
```

`onOutlineClick` should scroll the editor body to the matching section line (use `document.querySelector('[data-section-id="X"]')`).

In `home.scss`:

```scss
.home-shell__right-rail {
  width: 340px;
  padding: 32px;
  display: flex;
  flex-direction: column;
  gap: var(--bd-space-4);
  background: var(--bd-color-surface);
  border-left: 1px solid var(--bd-color-border);
  flex-shrink: 0;

  @media (max-width: 1280px) {
    display: none;   // hide on smaller desktops
  }
}
```

## Acceptance criteria
- Right column 340 px wide, visible at ≥ 1280 px viewport.
- Outline lists each top-level section heading; nested sections show indented.
- Clicking a row scrolls the matching `[data-section-id]` element into view; the row gets the active treatment until the user scrolls or clicks a different one.
- Section label "OUTLINE" matches design typography.
