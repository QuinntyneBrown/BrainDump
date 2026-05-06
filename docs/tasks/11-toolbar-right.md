# 11 — Toolbar right: visibility / history / share + Save tonal button

**Audit reference:** §3.2
**Design nodes:** `eWRGZ` ("dtBarRight") containing:
- `QW8sr` — `oaqpE` icon button, icon `visibility`, fill `$color-text-secondary`.
- `JdI1k` — `oaqpE` icon button, icon `history`.
- `AHuMl` — `oaqpE` icon button, icon `share`.
- `hKO8H` — `yfjZu` (Component/Button/Tonal) instance, label "Save".

## Goal
Replace the current `search` + `sign-out` actions with the four controls in the design.

## Files to touch
- `frontend/projects/brain-dump/src/app/home/home.ts` — update `topBarActions`.
- (Probably) `frontend/projects/components/src/lib/top-app-bar/top-app-bar.html` — to project a tonal Save button as the trailing action while keeping icon-button actions in the loop.

## Implementation

The cleanest approach: keep `actions` as the icon-button list, then add a `trailingButton` slot for the labelled Save button. In `top-app-bar.html`:

```html
…
<span class="bd-top-app-bar__spacer"></span>
@for (action of actions(); track action.id ?? action.icon) {
  <bd-icon-button
    [icon]="action.icon"
    [ariaLabel]="action.ariaLabel"
    (click)="actionClick.emit(action)"
  />
}
<ng-content select="[bdTopAppBarTrailing]" />
```

In `home.ts`:

```ts
protected readonly topBarActions: readonly BdTopAppBarAction[] = [
  { id: 'preview', icon: 'visibility', ariaLabel: 'Toggle preview' },
  { id: 'history', icon: 'history', ariaLabel: 'View history' },
  { id: 'share',   icon: 'share',    ariaLabel: 'Share' },
];
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
>
  <bd-button
    bdTopAppBarTrailing
    tone="tonal"
    (click)="onSave()"
    data-testid="save-document"
  >
    Save
  </bd-button>
</bd-top-app-bar>
```

In `home.ts`, swap `sign-out` handling. Sign-out belongs in the avatar menu (task 04 surface), not the toolbar. Add `onSave()` as a no-op stub that toasts "Saved" until persistence work lands.

```ts
protected onSave(): void {
  this.toast('Saved');
}

protected onTopBarAction(action: BdTopAppBarAction): void {
  switch (action.id) {
    case 'preview': /* TODO: toggle preview mode */ break;
    case 'history': /* TODO: open history panel */ break;
    case 'share':   /* TODO: open share dialog */   break;
  }
}
```

## Acceptance criteria
- Toolbar right shows: `visibility`, `history`, `share` icon buttons followed by a tonal "Save" button.
- Each icon button has a unique `aria-label`.
- Save button uses `BdButton` with `tone="tonal"` (task 06).
- Sign-out moves out of the toolbar — relocate to the avatar menu when wired (out of scope for this task; document in commit message).

## Dependencies
- Requires task 06 (`BdButton` tonal variant).
- Pairs with task 10 (toolbar left).
