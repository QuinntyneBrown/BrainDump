# 06 — Replace sidebar `+` icon button with "+ New" tonal button

**Audit reference:** §2.1
**Design node:** `V33Xc` — instance of `yfjZu` (Component/Button/Tonal); descendant override `ZEG8c.content = "+  New"`.

## Goal
Swap the circular `add` icon button in the "Notes" sidebar header with a labelled tonal button reading **"+ New"**.

## Design intent
- Tonal button (M3): `$color-primary-container` fill, `$color-primary` text, pill shape (`cornerRadius: $radius-full`), padding ~`[10, 16]`, label "+  New".
- Sits to the right of the "Notes" headline in the sidebar header (`q96orU`, `justifyContent: space_between`).

## Files to touch
- `frontend/projects/brain-dump/src/app/home/home.html`
- New component: `frontend/projects/components/src/lib/button/button.{ts,html,scss}` *(only if no shared button exists yet)*

## Decision
If the component library does not already have a labelled tonal button, create `BdButton` with a `tone` input (`tonal | filled | outlined | text`). If it exists, use it.

A grep for existing buttons:
```
frontend/projects/components/src/lib/**/button*.ts
```
Confirm before creating duplicate.

## Implementation

In `home.html` replace:

```html
<bd-icon-button
  icon="add"
  ariaLabel="Add section"
  tone="primary"
  (click)="onAddRootSection()"
  data-testid="add-section"
/>
```

with:

```html
<bd-button
  tone="tonal"
  leadingIcon="add"
  (click)="onAddRootSection()"
  data-testid="add-section"
>
  New
</bd-button>
```

If `BdButton` doesn't exist, sketch it as:

```ts
@Component({
  selector: 'bd-button',
  imports: [MatIcon],
  template: `
    @if (leadingIcon(); as i) { <mat-icon>{{ i }}</mat-icon> }
    <ng-content />
  `,
  host: {
    '[attr.data-tone]': 'tone()',
    '[attr.role]': '"button"',
    '[attr.tabindex]': '0',
  },
  styleUrl: './button.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdButton {
  readonly tone = input<'tonal' | 'filled' | 'outlined' | 'text'>('tonal');
  readonly leadingIcon = input<string | null>(null);
}
```

with SCSS that styles the host as a 40 px tall pill, ` $color-primary-container` fill for `tonal`, etc. Add to `public-api.ts`.

## Acceptance criteria
- Sidebar header shows a pill-shaped tonal button labelled "+ New" (with the `add` Material icon followed by "New").
- Click triggers `onAddRootSection()` (existing handler).
- Visual: matches the design's `Button/Tonal` — primary container background, primary text/icon, full-radius pill.
- `data-testid="add-section"` preserved so e2e tests keep passing.
