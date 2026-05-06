# 17 — Status bar footer

**Audit reference:** §6.1
**Design nodes:** `r0lBi1` (status bar, 32 tall, fill `$color-surface-2`, padding `[24, 0]`); left group `R27JvN` containing `T6QYRw` (saved indicator) and `xpzup` (branch); right group `PIE2V` containing 4 mono text fields.

## Goal
Render a sticky bottom status bar with save state + branch on the left, and `lines · markdown · encoding · Ln/Col` on the right.

## Design intent

**Left group:**
- `T6QYRw`: 14×14 `check_circle` icon (`$color-success`) + "Saved 2s ago" (`$color-text-secondary`, `$size-label-sm`).
- `xpzup`: 14×14 `merge_type` icon (`$color-text-tertiary`) + "main" (mono, `$color-text-tertiary`).
- Group gap: 16 px.

**Right group:**
- "15 lines" (mono, tertiary)
- "Markdown" (mono, tertiary)
- "UTF-8" (mono, tertiary)
- "Ln 15, Col 19" (mono, tertiary)
- Group gap: 16 px.

**Bar:** height 32, fill `$color-surface-2`, justify-between, padding `[24, 0]` (24 horizontal).

## Files to touch
- New component: `frontend/projects/components/src/lib/status-bar/status-bar.{ts,html,scss}`.
- `frontend/projects/components/src/public-api.ts`.
- `frontend/projects/brain-dump/src/app/home/home.html` — render at the bottom of `<main class="home-shell__editor">`.
- `frontend/projects/brain-dump/src/app/home/home.ts` — supply props.

## Implementation

### `BdStatusBar`

```ts
export interface BdStatusBarLeft {
  readonly saveState: 'saved' | 'saving' | 'dirty' | 'error';
  readonly savedAgo?: string | null;
  readonly branch?: string | null;
}

export interface BdStatusBarRight {
  readonly lines: number;
  readonly language: string;
  readonly encoding: string;
  readonly cursor: { line: number; col: number };
}

@Component({
  selector: 'bd-status-bar',
  imports: [MatIcon],
  templateUrl: './status-bar.html',
  styleUrl: './status-bar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdStatusBar {
  readonly left = input.required<BdStatusBarLeft>();
  readonly right = input.required<BdStatusBarRight>();
}
```

Template:

```html
<div class="bd-status-bar__left">
  <span class="bd-status-bar__group">
    <mat-icon class="bd-status-bar__icon" [attr.data-tone]="left().saveState">
      {{ saveIcon() }}
    </mat-icon>
    <span class="bd-status-bar__text">{{ saveLabel() }}</span>
  </span>
  @if (left().branch; as b) {
    <span class="bd-status-bar__group">
      <mat-icon class="bd-status-bar__icon" data-tone="muted">merge_type</mat-icon>
      <span class="bd-status-bar__text bd-status-bar__text--mono">{{ b }}</span>
    </span>
  }
</div>
<div class="bd-status-bar__right">
  <span class="bd-status-bar__text bd-status-bar__text--mono">{{ right().lines }} lines</span>
  <span class="bd-status-bar__text bd-status-bar__text--mono">{{ right().language }}</span>
  <span class="bd-status-bar__text bd-status-bar__text--mono">{{ right().encoding }}</span>
  <span class="bd-status-bar__text bd-status-bar__text--mono">
    Ln {{ right().cursor.line }}, Col {{ right().cursor.col }}
  </span>
</div>
```

Where `saveIcon()`/`saveLabel()` are computed from `left().saveState`:

```ts
protected readonly saveIcon = computed(() => {
  switch (this.left().saveState) {
    case 'saved': return 'check_circle';
    case 'saving': return 'sync';
    case 'dirty': return 'edit';
    case 'error': return 'error';
  }
});

protected readonly saveLabel = computed(() => {
  const l = this.left();
  if (l.saveState === 'saved') return l.savedAgo ? `Saved ${l.savedAgo}` : 'Saved';
  if (l.saveState === 'saving') return 'Saving…';
  if (l.saveState === 'dirty') return 'Unsaved changes';
  return 'Save failed';
});
```

Styles:

```scss
:host {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 32px;
  padding: 0 24px;
  background: var(--bd-color-surface-2);
  flex-shrink: 0;
}

.bd-status-bar__left,
.bd-status-bar__right {
  display: inline-flex;
  align-items: center;
  gap: 16px;
}

.bd-status-bar__group {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.bd-status-bar__icon {
  width: 14px;
  height: 14px;
  font-size: 14px;
  color: var(--bd-color-text-tertiary);

  &[data-tone='saved']  { color: var(--bd-color-success); }
  &[data-tone='saving'] { color: var(--bd-color-primary); }
  &[data-tone='dirty']  { color: var(--bd-color-warning, var(--bd-color-text-secondary)); }
  &[data-tone='error']  { color: var(--bd-color-error, #ff5d5d); }
}

.bd-status-bar__text {
  font-family: var(--bd-font-body);
  font-size: var(--bd-size-label-sm);
  color: var(--bd-color-text-secondary);
}

.bd-status-bar__text--mono {
  font-family: var(--bd-font-mono);
  color: var(--bd-color-text-tertiary);
}
```

If `--bd-color-success` doesn't exist, add it to the tokens file.

### Wire into Home

In `home.ts`:

```ts
protected readonly statusLeft = computed<BdStatusBarLeft>(() => ({
  saveState: 'saved',
  savedAgo: this.lastSavedAgo(),    // signal updated after each successful mutation
  branch: 'main',
}));

protected readonly statusRight = computed<BdStatusBarRight>(() => ({
  lines: this.lines().length,
  language: 'Markdown',
  encoding: 'UTF-8',
  cursor: { line: 1, col: 1 }, // TODO: real cursor position from Monaco when wired
}));
```

In `home.html`, after the editor body and inside `<main>`:

```html
<bd-status-bar
  [left]="statusLeft()"
  [right]="statusRight()"
  data-testid="status-bar"
/>
```

## Acceptance criteria
- A 32 px tall status bar sits at the bottom of the editor area, full-width within the editor column, with `--bd-color-surface-2` background.
- Left side shows a save indicator (icon + caption) and the branch ("main").
- Right side shows lines count, "Markdown", "UTF-8", and a Ln/Col indicator (cursor stubs to 1/1 for now).
- Status bar does not overlap the editor body — it sits below it.
