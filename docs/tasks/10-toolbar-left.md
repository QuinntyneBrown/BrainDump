# 10 — Toolbar left: document icon + facts badge + edited caption ✅ Completed

**Audit reference:** §3.1
**Design nodes:** `u7Xtj` ("dtBarLeft") containing:
- `wI9dA` — 20 px `description` icon, fill `$color-text-secondary`.
- `rUd1i` — "brain-dump.md" (`$font-display`, `$size-title-lg`, `$weight-semibold`, letter-spacing −0.5).
- `coxfh` — instance of `P6isad` (Component/Badge), label "facts".
- `PasJp` — "Edited 2 minutes ago" caption (`$color-text-tertiary`, `$size-label-md`).

## Goal
The editor toolbar's left side must mirror the design: leading document icon, document name, an inline "facts" badge, and a relative-time edit caption.

## Files to touch
- `frontend/projects/components/src/lib/top-app-bar/top-app-bar.{ts,html,scss}` — extend or replace.
- `frontend/projects/brain-dump/src/app/home/home.html` and `home.ts`.
- (Possibly new) `frontend/projects/components/src/lib/badge/badge.{ts,html,scss}`.

## Decision
The current `BdTopAppBar` is too generic for the design — it only supports `leadingIcon` + `title` + actions. Two options:

**A. Extend `BdTopAppBar`** with `leadingIcon`, `badge`, and `caption` inputs (plus content-projection slots).

**B. Replace it inside the home page** with a custom toolbar template directly in `home.html`.

Prefer **A** since the design system should own the toolbar shape. Only fall back to **B** if extending becomes invasive.

## Implementation (option A)

Extend `BdTopAppBar`:

```ts
export class BdTopAppBar {
  readonly title = input.required<string>();
  readonly leadingIcon = input<string | null>(null);
  readonly leadingAriaLabel = input<string>('');
  readonly badge = input<string | null>(null);
  readonly caption = input<string | null>(null);
  readonly actions = input<readonly BdTopAppBarAction[]>([]);
  // … existing outputs …
}
```

Template:

```html
<mat-toolbar class="bd-top-app-bar">
  @if (leadingIcon(); as i) {
    <mat-icon class="bd-top-app-bar__leading-icon">{{ i }}</mat-icon>
  }
  <h1 class="bd-top-app-bar__title">{{ title() }}</h1>
  @if (badge(); as b) {
    <bd-badge>{{ b }}</bd-badge>
  }
  @if (caption(); as c) {
    <span class="bd-top-app-bar__caption">{{ c }}</span>
  }
  <span class="bd-top-app-bar__spacer"></span>
  …
</mat-toolbar>
```

Styles:

```scss
.bd-top-app-bar__leading-icon {
  width: 20px;
  height: 20px;
  font-size: 20px;
  color: var(--bd-color-text-secondary);
}

.bd-top-app-bar__title {
  font-family: var(--bd-font-display);
  font-size: var(--bd-size-title-lg);
  font-weight: var(--bd-weight-semibold);
  letter-spacing: -0.5px;
  color: var(--bd-color-text-primary);
}

.bd-top-app-bar__caption {
  font-family: var(--bd-font-body);
  font-size: var(--bd-size-label-md);
  color: var(--bd-color-text-tertiary);
}
```

`BdBadge` (small pill, padding 2/6, fill `$color-surface-2`, text `$color-text-secondary`) — sketch:

```ts
@Component({ selector: 'bd-badge', template: `<ng-content />`, … })
export class BdBadge {}
```

```scss
:host {
  display: inline-flex;
  align-items: center;
  height: 20px;
  padding: 0 8px;
  border-radius: var(--bd-radius-full);
  background: var(--bd-color-surface-2);
  color: var(--bd-color-text-secondary);
  font-family: var(--bd-font-body);
  font-size: var(--bd-size-label-sm);
}
```

In `home.html`:

```html
<bd-top-app-bar
  title="brain-dump.md"
  leadingIcon="description"
  badge="facts"
  [caption]="lastEditedCaption()"
  [actions]="topBarActions"
  (actionClick)="onTopBarAction($event)"
/>
```

In `home.ts`:

```ts
protected readonly lastEditedCaption = computed(() => {
  const ts = this.lastModifiedAt();    // signal sourced from latest mutation
  return ts ? `Edited ${relativeTime(ts)}` : 'Up to date';
});
```

## Acceptance criteria
- Toolbar shows: `description` icon → "brain-dump.md" → small "facts" badge → "Edited Xm ago" caption → spacer → action icons (covered by task 11).
- Title typography matches the design tokens (display font, title-lg, semibold, −0.5 letter-spacing).
- Caption updates when a section/fact mutation succeeds.

## Dependencies
- Pairs with task 11 (toolbar right) — same component, same template.
