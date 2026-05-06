# 14 — Style heading markdown lines ✅ Completed

**Audit reference:** §4.3
**Design nodes:** `iiE0i`, `a1JhV`, `IA8Wd`, `acVFY` — `bd-monaco-line` instances with descendant override `nHkBz.fill = #A6C8FF`, `fontSize: 20` (h1) or `16` (h2), `fontWeight: $weight-semibold`.

## Goal
Heading lines (`# …`, `## …`) render larger and in primary-blue tint to mimic syntax-highlighted Markdown.

## Files to touch
- `frontend/projects/components/src/lib/monaco-line/monaco-line.{ts,html,scss}`
- `frontend/projects/brain-dump/src/app/home/home.ts` — pass a `tone`/`level` to each line.
- `frontend/projects/brain-dump/src/app/home/home.html`

## Implementation

### Add a `level` input to `BdMonacoLine`

```ts
export class BdMonacoLine {
  readonly lineNumber = input.required<number>();
  readonly code = input.required<string>();
  readonly level = input<0 | 1 | 2 | 3>(0); // 0 = body, 1 = h1, 2 = h2, 3 = h3
}
```

Bind a data attribute on the host:

```ts
host: {
  '[attr.data-level]': 'level()',
},
```

### CSS

In `monaco-line.scss`:

```scss
:host([data-level='1']) .bd-monaco-line__code {
  color: var(--bd-color-syntax-heading, #A6C8FF);
  font-size: 20px;
  font-weight: var(--bd-weight-semibold);
}

:host([data-level='2']) .bd-monaco-line__code {
  color: var(--bd-color-syntax-heading, #A6C8FF);
  font-size: 16px;
  font-weight: var(--bd-weight-semibold);
}

:host([data-level='3']) .bd-monaco-line__code {
  color: var(--bd-color-syntax-heading, #A6C8FF);
  font-size: 14px;
  font-weight: var(--bd-weight-semibold);
}
```

Add `--bd-color-syntax-heading` to the tokens file mapping to `#A6C8FF` (the design's literal).

### Compute level in `home.ts`

Update the `walk` closure in `lines()` to record heading level for section lines, and 0 for fact/blank:

```ts
const headingLevel = depth === 0 ? 1 : depth === 1 ? 2 : 3;
out.push({
  lineNumber: lineNo++,
  code: `${prefix} ${section.title}`,
  kind: 'section',
  sectionId: section.id,
  depth,
  level: headingLevel,
});
```

Add `level?: 0 | 1 | 2 | 3` to `RenderedLine`.

### Pass to template

```html
<bd-monaco-line
  [lineNumber]="line.lineNumber"
  [code]="line.code"
  [level]="line.level ?? 0"
/>
```

## Acceptance criteria
- `# brain-dump` renders at 20 px in `#A6C8FF`, semibold.
- `## Facts`, `## Authors`, `## Cadence` render at 16 px in `#A6C8FF`, semibold.
- Body lines and bullet lines remain at the default size/color.
