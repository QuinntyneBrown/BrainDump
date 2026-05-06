# 04 — Add avatar at bottom of side rail

**Audit reference:** §1.4
**Design nodes:** `x33bly` (flex spacer, fill_container) + `mBVXF` (40×40 ellipse, fill `$color-primary-container`).

## Goal
Pin a 40 px round avatar at the bottom of the navigation rail, separated from the nav items by a flex spacer.

## Design intent
- Element after the four nav items: a vertical spacer that fills remaining space.
- Below the spacer: a 40×40 ellipse (`mBVXF`), `$color-primary-container` fill, `align-self: center`.
- No initials/text inside the avatar in the design — placeholder only. (We can render the user's initial later when wiring to auth state.)

## Files to touch
- `frontend/projects/components/src/lib/side-rail/side-rail.scss`
- `frontend/projects/brain-dump/src/app/home/home.html`
- (Optionally) a tiny new `BdAvatar` component in `frontend/projects/components/src/lib/avatar/` if reuse is foreseen — otherwise inline a `<div class="home-shell__rail-avatar">` is fine for now.

## Implementation

In `home.html`, after the four `<bd-nav-item>` rows inside `<bd-side-rail>`:

```html
<span class="home-shell__rail-spacer"></span>
<div class="home-shell__rail-avatar" aria-label="Account">{{ initials() }}</div>
```

In `home.ts`, expose an `initials` computed signal:

```ts
protected readonly initials = computed(() => {
  const email = this.authService.currentEmail();
  return email ? email.charAt(0).toUpperCase() : '';
});
```

(If `AUTH_SERVICE` doesn't yet expose `currentEmail`, stub with `''`; the design has no glyph so an empty avatar is acceptable.)

In `home.scss`:

```scss
.home-shell__rail-spacer {
  flex: 1 1 auto;
}

.home-shell__rail-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: var(--bd-color-primary-container);
  color: var(--bd-color-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-family: var(--bd-font-body);
  font-size: var(--bd-size-label-md);
  font-weight: var(--bd-weight-medium);
  align-self: center;
  margin-bottom: var(--bd-space-3);
}
```

To make `<bd-side-rail>` pass through children that aren't `<bd-nav-item>`, no change is needed — it's already `<ng-content />`.

## Acceptance criteria
- A 40 px circle is visible at the bottom of the rail, centered horizontally.
- A flex spacer pushes it to the bottom regardless of how many nav items render.
- Avatar fill matches `$color-primary-container`.

## Out of scope
- Wiring real user identity / image. The design ships an empty primary-container disc.
