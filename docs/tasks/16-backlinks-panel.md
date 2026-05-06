# 16 — Backlinks panel

**Audit reference:** §5
**Design nodes:** `IxtqI` ("BACKLINKS" label) + `lQquH` (`dtBL`) → cards `ginzQ`, `KosMR`. A divider rectangle `flOjh` separates outline from backlinks.

## Goal
Below the outline panel, render a "BACKLINKS" section listing documents that reference the current note.

## Design intent

**Section label** (`IxtqI`):
- "BACKLINKS", uppercase, `$font-body`, `$size-label-sm`, `$weight-semibold`, letter-spacing 1, fill `$color-text-tertiary`.

**Backlink card** (`ginzQ`):
- vertical layout, gap 4, padding 12, fill `$color-surface-2`, `cornerRadius: $radius-md`.
- title text: `$color-text-primary`, `$weight-medium`, `$size-body-md`.
- caption text: `$color-text-tertiary`, `$size-label-sm`.

## Files to touch
- New component: `frontend/projects/components/src/lib/backlinks/backlinks.{ts,html,scss}`.
- `frontend/projects/components/src/public-api.ts`.
- `frontend/projects/brain-dump/src/app/home/home.html` — add inside the right rail, after the outline.

## Implementation

### `BdBacklinks`

```ts
export interface BdBacklinkEntry {
  readonly id: string | number;
  readonly title: string;
  readonly excerpt: string;
}

@Component({
  selector: 'bd-backlinks',
  templateUrl: './backlinks.html',
  styleUrl: './backlinks.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdBacklinks {
  readonly entries = input.required<readonly BdBacklinkEntry[]>();
  readonly entryClick = output<BdBacklinkEntry>();
}
```

Template:

```html
<h3 class="bd-backlinks__heading">BACKLINKS</h3>
<div class="bd-backlinks__list">
  @for (e of entries(); track e.id) {
    <button
      type="button"
      class="bd-backlinks__card"
      (click)="entryClick.emit(e)"
    >
      <span class="bd-backlinks__title">{{ e.title }}</span>
      <span class="bd-backlinks__excerpt">{{ e.excerpt }}</span>
    </button>
  }
</div>
```

Styles:

```scss
:host {
  display: flex;
  flex-direction: column;
  gap: var(--bd-space-2);
}

.bd-backlinks__heading {
  margin: 0;
  font-family: var(--bd-font-body);
  font-size: var(--bd-size-label-sm);
  font-weight: var(--bd-weight-semibold);
  letter-spacing: 1px;
  color: var(--bd-color-text-tertiary);
  text-transform: uppercase;
}

.bd-backlinks__list {
  display: flex;
  flex-direction: column;
  gap: var(--bd-space-2);
}

.bd-backlinks__card {
  all: unset;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 12px;
  border-radius: var(--bd-radius-md);
  background: var(--bd-color-surface-2);
}

.bd-backlinks__title {
  font-family: var(--bd-font-body);
  font-size: var(--bd-size-body-md);
  font-weight: var(--bd-weight-medium);
  color: var(--bd-color-text-primary);
}

.bd-backlinks__excerpt {
  font-family: var(--bd-font-body);
  font-size: var(--bd-size-label-sm);
  color: var(--bd-color-text-tertiary);
}
```

### Wire into Home

In `home.ts`, expose a stub list until backlink data is modelled:

```ts
protected readonly backlinks = signal<readonly BdBacklinkEntry[]>([]);
// TODO: derive from documents that link to brain-dump.md
```

In `home.html`, add inside the right rail (under the outline):

```html
<hr class="home-shell__rail-divider" />
<bd-backlinks [entries]="backlinks()" />
```

In `home.scss`:

```scss
.home-shell__rail-divider {
  border: 0;
  height: 1px;
  background: var(--bd-color-border-subtle);
  margin: 0;
}
```

## Acceptance criteria
- Section label "BACKLINKS" renders in tertiary text matching the design typography.
- A divider sits between the outline and the backlinks list.
- The component renders an empty state cleanly (no list, no divider rendered when empty — adjust as needed).
- Cards show title + excerpt; cards are buttons (keyboard-focusable) and emit `entryClick`.

## Backlog
- Real backlink data: derive from documents that include `brain-dump.md` references. Out of scope for this task.
