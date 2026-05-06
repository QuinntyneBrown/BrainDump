# Home Page UI Audit

**Date:** 2026-05-06
**Implementation:** `http://localhost:4200/home` — `frontend/projects/brain-dump/src/app/home/*`
**Reference design:** `docs/user-interface-designs/design.pen` → frame `XL - Desktop - Editor` (`A40kPP`, 1920×1080)

This audit compares each region of the running home page against the authoritative .pen design. Findings are grouped by region and ordered roughly top-left → bottom-right. Each entry lists the **expected** state (from the design), the **actual** state (from the implementation), and the **fix location**.

---

## 1. Left navigation rail (`bd-side-rail`)

The design's nav rail is an 80 px column (frame `t2jT2`) with: an 8 px top spacer, a hamburger icon button, a 16 px gap, four stacked nav items (Notes / Tags / Recent / Settings), a flex spacer, and a circular avatar pinned at the bottom.

### 1.1 Hamburger button is missing

- **Design:** First interactive element in the rail is an `oaqpE` (Component/Button/Icon) instance with the `menu` icon (node `HcPVc`), sitting above the nav items.
- **Actual:** No hamburger button is rendered. `home.html:2-7` lists only the four `<bd-nav-item>`s.
- **Fix:** Add a `<bd-icon-button icon="menu" ariaLabel="…" />` (or equivalent) as the first child of `<bd-side-rail>` in `frontend/projects/brain-dump/src/app/home/home.html`.

### 1.2 Nav-item labels (Notes / Tags / Recent / Settings) are hidden

- **Design:** Each nav item (`pI3ZX`, `rDt7t`, `aLyQ2`, `uQ0Ue`) is a 64×64 vertical frame containing a 24 px icon **and** a label below it. Active item shows the label in primary color.
- **Actual:** `frontend/projects/components/src/lib/nav-item/nav-item.scss:52-64` — when `inRail` is true, `.bd-nav-item__label { display: none; }`. Only the icon is visible.
- **Fix:** Remove the `display: none` from rail mode. Restore the vertical icon-over-label layout (gap ~4 px, font-size matching `--bd-size-label-sm`).

### 1.3 Active highlight is a circle, should be a rounded square (M3 nav rail "indicator")

- **Design:** Active item (`pI3ZX`, "Notes") uses `cornerRadius: $radius-xl` on a 64×64 frame — a rounded square pill of fixed width matching the icon row, *not* a perfect circle. The pill wraps the icon row (24 px); the label sits below the pill, unhighlighted.
- **Actual:** `nav-item.scss:59` sets `border-radius: 50%` on the entire 48×48 host. The highlight is a circle and centers on the whole component, not just the icon.
- **Fix:** Two parts.
  1. Drop `border-radius: 50%` and re-introduce a fixed 56×32 (or similar) pill around just the icon. Easiest path: wrap the icon in an inner element that takes the active background, leaving the label outside the pill.
  2. Use `--bd-radius-xl` (or `border-radius: 16px`) instead of `50%`.

### 1.4 Avatar at bottom of rail is missing

- **Design:** `mBVXF` — a 40×40 ellipse filled with `$color-primary-container`, pinned to the bottom of the rail by a flex-grow spacer (`x33bly`).
- **Actual:** No avatar element rendered.
- **Fix:** Add a flex-spacer plus an avatar slot at the bottom of `<bd-side-rail>`. Either add an avatar projection slot to `BdSideRail`, or render `<div class="home-shell__rail-avatar"></div>` as the last child in `home.html`.

### 1.5 Rail width is 64 px, design specifies 80 px

- **Design:** `t2jT2.width = 80`, padding `[0, 12]` — a Material 3 navigation rail width.
- **Actual:** `frontend/projects/components/src/lib/side-rail/side-rail.scss:6` — `width: 64px`.
- **Fix:** Update `width: 64px` → `width: 80px`. Padding looks reasonable but may need adjustment to `0 var(--bd-space-3)`.

### 1.6 Rail gap and item spacing

- **Design:** Gap between nav items is `$space-2` (8 px). Items are 64 px tall.
- **Actual:** `side-rail.scss:5` — `gap: var(--bd-space-1)` (4 px). Rail items in rail mode are 48×48 (`nav-item.scss:53-54`), not 64×64.
- **Fix:** Bump rail gap to `--bd-space-2`. Increase rail-mode nav-item height to ~64 px so icon + label both fit.

---

## 2. Note list / sidebar (`bd-sidebar`)

Design region: `U0HeWq` ("note list"), 360 px wide, fill `$color-surface-2`, padding 24, gap `$space-4`.

### 2.1 "+ New" header action is an icon button, should be a tonal text button

- **Design:** `V33Xc` is a `yfjZu` (Component/Button/Tonal) instance with descendant override `ZEG8c.content = "+  New"` — a labelled tonal button reading **"+ New"** sitting to the right of the "Notes" headline.
- **Actual:** `home.html:11-18` renders a `<bd-icon-button icon="add" tone="primary">` — a 40 px circular icon button with no label.
- **Fix:** Replace the `<bd-icon-button>` with a tonal button component bearing the label `"+ New"` (or `"New +"` per the user's note — match whichever the design surfaces). If a `bd-button` tonal variant doesn't exist yet, create one or use Material's `mat-button` with the tonal palette.

### 2.2 Search field is missing

- **Design:** Below the "Notes" header is `hxSEx`, an instance of `pJc4a` (Component/TextField) stretched to `fill_container` — the "Search notes…" search input visible at top of the sidebar in the screenshot.
- **Actual:** No search field rendered. The sidebar jumps from heading directly to note items.
- **Fix:** Add a search text field below the header in `home.html`. Wire to a no-op signal for now if backend filtering isn't implemented.

### 2.3 Filter chips ("All", "#facts", "#wip") are missing

- **Design:** Frame `qAuJQ` ("dtChips") holds three `sDkRY` (Component/Chip) instances: "All" (selected — primary container), "#facts", "#wip".
- **Actual:** No chip row is rendered.
- **Fix:** Add a chip row (using a `bd-chip` component if one exists, or a simple `<div class="chips">` with chip buttons) between the search field and the note list.

### 2.4 Note items missing structure: document icon, tags, last-updated caption

This is the user-flagged issue: "There should be document icon beside brain-dump.md with tags to the right and a little last updated caption."

- **Design:** Each `ctGht`/`r0rw0` (NoteItem) instance contains a vertical stack of three text fields — title, preview, and meta (e.g. "Today, 9:14"). The active item also surfaces metadata (the "Edited 2 minutes ago" timestamp appears in the toolbar but the active note row in the sidebar shows the date in `lbAaS`).
- **Actual:** `frontend/projects/components/src/lib/note-item/note-item.html` renders title + preview + meta, but the implementation passes `meta = '<n> facts'` (`home.ts:30`) instead of a relative timestamp like `"Today, 9:14"` or `"2h ago"`. There is **no document icon** (`description` Material symbol) preceding the title, and **no tag chips** displayed within the item.
- **Fix:**
  1. Add a leading `description` Material icon to `note-item.html` next to the title.
  2. Add a tags slot/input to `BdNoteItem` and a row of small tag chips ("#facts", "#wip", etc.) on the right of the title row.
  3. Replace the fact-count meta with a relative-time caption derived from the section's `updatedAt` field (will require API support — flag as backlog if not present).

### 2.5 Note list scrollbar visible (cosmetic)

- **Design:** No scrollbar artwork in the design.
- **Actual:** `bd-sidebar` body uses `overflow-y: auto` (sidebar.scss:32) which will surface a vertical scrollbar when content overflows. Acceptable as a desktop convention but noted for completeness.
- **Fix:** None required unless the team wants `overflow-y: overlay` (Webkit-only) or a styled scrollbar.

---

## 3. Editor toolbar (top of code area)

Design region: `uTvUy` ("toolbar"), 64 px tall, padding `[24, 0]`, fill `space_between`.

### 3.1 Document icon and "Edited 2 minutes ago" caption are missing

User-flagged: "There should be document icon beside brain-dump.md … and a little last updated caption."

- **Design (`u7Xtj` "dtBarLeft"):** horizontal row containing
  - 20 px `description` icon (`wI9dA`)
  - "brain-dump.md" headline (title-lg, semibold, primary text)
  - a "facts" badge (`coxfh` → `P6isad` Component/Badge)
  - "Edited 2 minutes ago" tertiary-text caption (`PasJp`)
- **Actual:** `home.html:38-43` passes only `title="brain-dump.md"` to `<bd-top-app-bar>`. There is no leading icon (the input `leadingIcon` is explicitly set to `null`), no badge, and no edited-time caption.
- **Fix:** Either extend `BdTopAppBar` with a leading-icon input (re-enabling the `description` icon) and a sub-title slot for the edited caption, or replace the top app bar with a custom toolbar template inside `home.html` that mirrors the design exactly.

### 3.2 Toolbar right-side actions don't match

- **Design (`eWRGZ` "dtBarRight"):** four trailing controls in this order — `visibility`, `history`, `share` (each rendered with `oaqpE` Component/Button/Icon), then a tonal **"Save"** button (`hKO8H`).
- **Actual:** `home.ts:82-85` declares only `search` + `sign-out` icon actions; no Save button, no visibility/history/share icons.
- **Fix:** Update `topBarActions` to surface the four design actions, or rebuild the toolbar to render the icon trio plus a real `bd-button[tone=tonal] label="Save"`.

### 3.3 Toolbar uses Angular Material `mat-toolbar`; design uses no fill / divider underneath

- **Design:** Toolbar has no fill (transparent over `S1v8d` "code area" surface), with a 1 px `$color-border-subtle` divider rectangle (`hJQor`) immediately below.
- **Actual:** `bd-top-app-bar` wraps a `<mat-toolbar>` which inherits Material 3 surface tones. The implementation does not render an explicit underline divider.
- **Fix:** Strip the toolbar fill (transparent), add a 1 px divider line below the toolbar matching `--bd-color-border-subtle`.

---

## 4. Editor body (markdown lines)

Design region: `Tby2u` ("editor body"), padding `[48, 80]`, gap 8.

### 4.1 Padding is much smaller than design

- **Design:** Editor body padding `[48, 80]` — 48 px vertical, 80 px horizontal.
- **Actual:** `home.scss:43` — `padding: var(--bd-space-6) var(--bd-space-4)` (typically 24 px vertical, 16 px horizontal).
- **Fix:** Increase to `padding: 48px 80px` or token equivalents (e.g. `var(--bd-space-12) var(--bd-space-20)` if such tokens exist).

### 4.2 Line gap 8 px in design vs. 2 px in implementation

- **Design:** `gap: 8` between lines.
- **Actual:** `home.scss:46` — `gap: 2px`.
- **Fix:** Bump gap to 8 px (or token).

### 4.3 Heading line styling

- **Design:** Heading lines (`# brain-dump`, `## Facts`) use `fill: #A6C8FF` (a primary-tinted blue), `fontSize: 20`/`16`, weight `$weight-semibold`. The `bd-monaco-line` accepts these style overrides via descendant `nHkBz` overrides.
- **Actual:** `bd-monaco-line` likely renders all lines uniformly. Need to confirm — but visually the running app does not differentiate heading lines from body lines.
- **Fix:** Add a `kind`/`tone` input to `bd-monaco-line` so headings can take a different size/color. Drive it from `home.ts` based on whether the line starts with `#`.

---

## 5. Outline + Backlinks panel (right rail)

User-flagged: "There should be an outline component and backlinks component."

Design region: `wAvSZ` ("outline"), 340 px wide, fill `$color-surface`, padding 32, vertical layout, gap `$space-4`.

### 5.1 The entire right-side panel is missing

- **Design:** Right column of the editor area carries two stacked sections:
  - **OUTLINE** label (uppercase, tertiary text, letter-spacing 1) followed by `O0yO0R` (`dtOL`) — four list rows, one per heading: "brain-dump" (active, primary container), "Facts", "Authors", "Cadence". Each row has a leading `tag` icon and the heading text.
  - A divider rectangle (`flOjh`).
  - **BACKLINKS** label followed by `lQquH` (`dtBL`) — two cards titled "Architecture decisions" and "API surface notes" with secondary captions ("…sources from brain-dump.md", "…facts mirror brain-dump.md").
- **Actual:** Not rendered. `home.html` ends with the editor `<main>`; no third column exists.
- **Fix:**
  1. Create `BdOutline` and `BdBacklinks` components in `frontend/projects/components` (or a single `BdRightRail` that renders both sections).
  2. Add a third column in `home.html` after `<main class="home-shell__editor">`.
  3. Wire the outline list from `home.ts` by scanning the rendered lines for `kind === 'section'`. Backlinks data is not yet modelled; can be stubbed.

---

## 6. Status bar (footer of the document)

User-flagged: "There should be a footer component on the document with information about when the brain-dump was saved, number of lines, encoding, line and col numbers, etc..."

Design region: `r0lBi1` ("status bar"), 32 px tall, fill `$color-surface-2`, padding `[24, 0]`, `space_between` justified.

### 6.1 The entire status bar is missing

- **Design (left side `R27JvN`):**
  - `T6QYRw`: green `check_circle` icon + "Saved 2s ago" caption
  - `xpzup`: `merge_type` icon + "main" branch text (mono)
- **Design (right side `PIE2V`):**
  - "15 lines" (mono, tertiary)
  - "Markdown" (mono, tertiary)
  - "UTF-8" (mono, tertiary)
  - "Ln 15, Col 19" (mono, tertiary)
- **Actual:** No status-bar element exists. The editor body extends to the bottom of the viewport. The FAB (`home-shell__fab`) is the only thing pinned to the bottom right — and the design has no FAB at all.
- **Fix:**
  1. Create a `BdStatusBar` component with two slots (left, right) or with structured inputs.
  2. Render it as the last child of `<main class="home-shell__editor">`, sticky to the bottom.
  3. Populate left side with save-state and branch indicators (use stubbed values until the API supplies them — `Saved Xs ago` can derive from the most recent successful section/fact mutation).
  4. Populate right side with `lines().length`, "Markdown", "UTF-8", and a derived `Ln X, Col Y` (cursor position will need to come from the Monaco integration when wired up; stub for now).

### 6.2 FAB ("+") in the implementation is not in the design

- **Design:** No floating action button anywhere on this screen. Section creation is performed via the "+ New" tonal button in the sidebar header.
- **Actual:** `home.html:87-94` renders `<bd-fab icon="add">` pinned to the bottom-right.
- **Fix:** Remove the FAB and rely on the sidebar's "+ New" button (once implemented per §2.1) for adding sections.

---

## 7. Layout shell

### 7.1 Three-column layout, design is four-column

- **Design (top-level `A6Mq8` "editor"):** vertical layout with two children
  - `S1v8d` "code area" — horizontal layout: rail (80) + note list (360) + editor + outline/backlinks (340)
  - `r0lBi1` "status bar" — 32 px tall row at the bottom
- **Actual:** `home.html` renders three columns: rail, sidebar, editor. There is no fourth column for outline/backlinks and no status-bar row beneath the columns.
- **Fix:** Restructure `.home-shell` to nest a horizontal "code area" inside a vertical wrapper that also holds the status bar. Add the fourth (outline/backlinks) column.

### 7.2 Background tokens

- **Design:** Page bg = `$color-bg`; rail = `$color-surface`; note list = `$color-surface-2`; code area = `$color-surface`; outline = `$color-surface`; status bar = `$color-surface-2`.
- **Actual:** Most tokens look right at a glance, but the code-area / editor body uses `var(--bd-color-bg)` (`home.scss:47`) instead of `$color-surface`. Verify against the design system tokens.
- **Fix:** Switch `.home-shell__editor-body` background to `var(--bd-color-surface)`.

---

## 8. Cross-cutting / smaller items

| # | Detail | Design | Actual | Fix |
|---|---|---|---|---|
| 8.1 | Sidebar heading uses `$font-display`, `$size-headline-sm`, `$weight-semibold`, letter-spacing −0.5 | `$font-display` semibold | `bd-sidebar__title` uses `bold` weight (`sidebar.scss:25`) | Switch to `var(--bd-weight-semibold)` and add `letter-spacing: -0.5px`. |
| 8.2 | Sidebar heading text "Notes" is title-case | "Notes" | matches | OK |
| 8.3 | Note item meta says "Today, 9:14" / "Yesterday" / "3d ago" | relative timestamp | "<n> facts" | Switch meta value source. |
| 8.4 | Active note item uses `$color-primary-container` background and primary text | colored card | matches when `[active]` is true, but no item is currently set as active | Track selected note in `home.ts` and bind `[active]` accordingly. |

---

## Summary checklist

- [x] Add hamburger button at top of side rail
- [x] Show nav-item labels under icons in rail mode
- [x] Change rail active highlight from circle to rounded square (M3 indicator)
- [x] Add avatar at bottom of side rail
- [x] Widen rail to 80 px, increase rail-item height to ~64 px
- [x] Replace sidebar header `+` icon button with "+ New" tonal text button
- [x] Add search field below sidebar header
- [x] Add filter chips row (All, #facts, #wip)
- [x] Add document icon, tag chips, and relative-time caption to each note item
- [x] Add `description` icon, `facts` badge, and "Edited Xm ago" caption to the editor toolbar left
- [x] Add visibility / history / share icon buttons + Save tonal button to toolbar right
- [x] Add 1 px divider under toolbar; remove toolbar fill
- [x] Increase editor body padding to 48 / 80 and line gap to 8 px
- [x] Style heading markdown lines (#, ##) with primary blue tint and larger size
- [ ] Add Outline panel (340 px right column)
- [ ] Add Backlinks panel (below outline)
- [ ] Add status bar footer (Saved/branch + lines/encoding/Ln,Col)
- [ ] Remove the FAB
- [ ] Fix editor-body background token (surface, not bg)
- [ ] Tighten sidebar title weight to semibold + −0.5 letter-spacing
