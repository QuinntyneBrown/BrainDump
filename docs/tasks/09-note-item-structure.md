# 09 — Enrich note item with icon, tags, and timestamp

**Audit reference:** §2.4
**Design nodes:** `ctGht` (Component/NoteItem), `r0rw0` (Component/NoteItem/Active).

## Goal
Each note item in the sidebar should show:
1. A leading `description` Material icon next to the title.
2. A row of small tag chips on the right of the title row (e.g. `#facts`, `#wip`).
3. A relative-time caption ("Today, 9:14", "Yesterday", "3d ago") in place of the current "<n> facts" string.

## Design intent
NoteItem layout (from `ctGht`):
- vertical stack, gap 4, padding `$space-4`, fill `$color-surface`, `cornerRadius: $radius-md`.
- title row: `description` icon + title text + (right-aligned) tag chips.
- preview text: `$color-text-tertiary`, `$size-label-md`.
- meta text (timestamp): `$color-text-disabled`, `$size-label-sm`.

Active variant `r0rw0`: fill `$color-primary-container`, title in `$color-primary` `$weight-semibold`.

## Files to touch
- `frontend/projects/components/src/lib/note-item/note-item.html`
- `frontend/projects/components/src/lib/note-item/note-item.scss`
- `frontend/projects/components/src/lib/note-item/note-item.ts`
- `frontend/projects/brain-dump/src/app/home/home.html`
- `frontend/projects/brain-dump/src/app/home/home.ts`

## Implementation

### Component update (`note-item.ts`)

Add inputs for icon, tags, and a typed timestamp:

```ts
export class BdNoteItem {
  readonly title = input.required<string>();
  readonly preview = input<string | null>(null);
  readonly meta = input<string | null>(null);
  readonly tags = input<readonly string[]>([]);
  readonly icon = input<string>('description');
  readonly active = input(false);
}
```

### Template (`note-item.html`)

```html
<div class="bd-note-item__row">
  <mat-icon class="bd-note-item__icon">{{ icon() }}</mat-icon>
  <span class="bd-note-item__title">{{ title() }}</span>
  @if (tags().length) {
    <span class="bd-note-item__tags">
      @for (t of tags(); track t) {
        <span class="bd-note-item__tag">#{{ t }}</span>
      }
    </span>
  }
</div>
@if (preview(); as p) {
  <span class="bd-note-item__preview">{{ p }}</span>
}
@if (meta(); as m) {
  <span class="bd-note-item__meta">{{ m }}</span>
}
```

### Styles (`note-item.scss`)

```scss
.bd-note-item__row {
  display: flex;
  align-items: center;
  gap: var(--bd-space-2);
}

.bd-note-item__icon {
  font-size: 18px;
  width: 18px;
  height: 18px;
  color: var(--bd-color-text-secondary);
}

.bd-note-item__title {
  flex: 1 1 auto;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-family: var(--bd-font-body);
  font-size: var(--bd-size-body-md);
  font-weight: var(--bd-weight-medium);
  color: var(--bd-color-text-primary);
}

.bd-note-item__tags {
  display: inline-flex;
  gap: 4px;
  flex-shrink: 0;
}

.bd-note-item__tag {
  font-family: var(--bd-font-mono);
  font-size: var(--bd-size-label-sm);
  color: var(--bd-color-text-tertiary);
}

:host([data-active='true']) .bd-note-item__icon { color: var(--bd-color-primary); }
:host([data-active='true']) .bd-note-item__title { color: var(--bd-color-primary); font-weight: var(--bd-weight-semibold); }
```

### Caller (`home.ts` + `home.html`)

In `home.ts`, change `SectionSummary` to surface a relative timestamp and tags. Until the API supplies `updatedAt`/tags, stub with computed-from-fact-count placeholders so the UI is not blocked:

```ts
interface SectionSummary {
  readonly section: SectionDto;
  readonly preview: string;
  readonly tags: readonly string[];
  readonly meta: string;
}
```

Replace `meta: factCount + ' fact(s)'` with `meta: relativeTime(section.updatedAt ?? section.createdAt)`. If neither field exists, use `''` and add a TODO referencing this task.

Implement `relativeTime`:

```ts
function relativeTime(iso: string | undefined): string {
  if (!iso) return '';
  const then = new Date(iso).getTime();
  const diffMs = Date.now() - then;
  const minutes = Math.floor(diffMs / 60_000);
  if (minutes < 1) return 'Just now';
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days === 1) return 'Yesterday';
  if (days < 7) return `${days}d ago`;
  const weeks = Math.floor(days / 7);
  return `${weeks}w ago`;
}
```

In `home.html`, pass tags + meta:

```html
<bd-note-item
  [title]="s.section.title"
  [preview]="s.preview"
  [meta]="s.meta"
  [tags]="s.tags"
  [active]="false"
  [attr.data-testid]="'sidebar-section-' + s.section.id"
/>
```

## Acceptance criteria
- Each note item shows a `description` icon to the left of the title.
- Tag chips (when supplied) appear to the right of the title.
- The bottom caption is a relative-time string ("Today, 9:14" / "2h ago" / "3d ago"), not a fact-count.
- Active variant tints the icon and title in primary.

## Backlog (out of scope)
- Persisting `updatedAt` on `Section`.
- Persisting tags per section.
- Filtering by tag chip selection.
