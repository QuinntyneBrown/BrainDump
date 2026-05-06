import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';
import {
  DOCUMENTS_SERVICE,
  DocumentDto,
  FACTS_SERVICE,
  FactDto,
  FOLDERS_SERVICE,
  SECTIONS_SERVICE,
  SectionDto,
  TABS_SERVICE,
  TabStateDto,
  TreeDto,
  WORKSPACE_SERVICE,
  WorkspaceDto,
} from 'api';
import {
  BdBacklinkEntry,
  BdBacklinks,
  BdButton,
  BdChip,
  BdConfirmDialog,
  BdConfirmDialogData,
  BdDocumentEditor,
  BdEditorLine,
  BdIconButton,
  BdNavItem,
  BdOutline,
  BdOutlineEntry,
  BdPageTree,
  BdPromptDialog,
  BdPromptDialogData,
  BdSideRail,
  BdSidebar,
  BdSnackbar,
  BdSplitEditor,
  BdStatusBar,
  BdStatusBarLeft,
  BdStatusBarRight,
  BdTab,
  BdTabBar,
  BdTextField,
  BdTopAppBar,
  BdTopAppBarAction,
  FactMenuAction,
  SectionMenuAction,
} from 'components';

interface Pane {
  readonly tabs: readonly number[];
  readonly activeIndex: number;
}

const EMPTY_TREE: TreeDto = { sections: [], facts: [] };

@Component({
  selector: 'app-home',
  imports: [
    BdTopAppBar,
    BdSideRail,
    BdSidebar,
    BdNavItem,
    BdIconButton,
    BdButton,
    BdTextField,
    BdChip,
    BdOutline,
    BdBacklinks,
    BdStatusBar,
    BdPageTree,
    BdTabBar,
    BdSplitEditor,
    BdDocumentEditor,
  ],
  templateUrl: './home.html',
  styleUrl: './home.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Home {
  private readonly workspaceService = inject(WORKSPACE_SERVICE);
  private readonly documentsService = inject(DOCUMENTS_SERVICE);
  private readonly foldersService = inject(FOLDERS_SERVICE);
  private readonly sectionsService = inject(SECTIONS_SERVICE);
  private readonly factsService = inject(FACTS_SERVICE);
  private readonly tabsService = inject(TABS_SERVICE);
  private readonly dialog = inject(MatDialog);
  private readonly snackbar = inject(MatSnackBar);

  protected readonly workspace = signal<WorkspaceDto>({ folders: [], documents: [] });
  protected readonly panes = signal<readonly Pane[]>([{ tabs: [], activeIndex: -1 }]);
  protected readonly focusedPaneIndex = signal<0 | 1>(0);
  protected readonly loading = signal(true);
  protected readonly searchQuery = signal('');
  protected readonly filter = signal<'all' | 'facts' | 'wip'>('all');
  protected readonly lastModifiedAt = signal<number | null>(null);

  /** Per-document tree cache. Mutations invalidate by replacing the entry. */
  private readonly treeByDoc = signal<ReadonlyMap<number, TreeDto>>(new Map());
  /** Suppresses the persist effect until the initial /api/tabs read completes. */
  private readonly tabsLoaded = signal(false);
  private persistTimer: ReturnType<typeof setTimeout> | null = null;

  protected readonly splitActive = computed(() => this.panes().length === 2);

  protected readonly leftPane = computed(() => this.panes()[0] ?? { tabs: [], activeIndex: -1 });
  protected readonly rightPane = computed<Pane | null>(() => this.panes()[1] ?? null);

  protected readonly leftTabs = computed<readonly BdTab[]>(() => this.tabsFor(this.leftPane()));
  protected readonly rightTabs = computed<readonly BdTab[]>(() => {
    const p = this.rightPane();
    return p ? this.tabsFor(p) : [];
  });

  protected readonly leftActiveDocId = computed<number | null>(() => activeDocId(this.leftPane()));
  protected readonly rightActiveDocId = computed<number | null>(() => {
    const p = this.rightPane();
    return p ? activeDocId(p) : null;
  });

  protected readonly leftLines = computed<readonly BdEditorLine[]>(() =>
    this.linesFor(this.leftActiveDocId()),
  );
  protected readonly rightLines = computed<readonly BdEditorLine[]>(() =>
    this.linesFor(this.rightActiveDocId()),
  );

  protected readonly focusedDocId = computed<number | null>(() => {
    const idx = this.focusedPaneIndex();
    return idx === 0 ? this.leftActiveDocId() : this.rightActiveDocId();
  });

  protected readonly editorTitle = computed(() => {
    const id = this.focusedDocId();
    if (id === null) return '—';
    return this.workspace().documents.find(d => d.id === id)?.title ?? '—';
  });

  protected readonly lastEditedCaption = computed(() => {
    const ts = this.lastModifiedAt();
    if (ts === null) return null;
    return `Edited ${formatRelativeTime(Date.now() - ts)}`;
  });

  protected readonly outlineEntries = computed<readonly BdOutlineEntry[]>(() => {
    const id = this.focusedDocId();
    if (id === null) return [];
    const tree = this.treeByDoc().get(id) ?? EMPTY_TREE;
    return buildLines(tree)
      .filter(l => l.kind === 'section' && l.sectionId !== undefined)
      .map<BdOutlineEntry>(l => ({
        id: l.sectionId!,
        label: tree.sections.find(s => s.id === l.sectionId)?.title ?? '',
        level: clampLevel((l.depth ?? 0) + 1),
      }));
  });

  protected readonly activeOutlineId = signal<number | null>(null);
  protected readonly backlinks = signal<readonly BdBacklinkEntry[]>([]);

  protected readonly statusLeft = computed<BdStatusBarLeft>(() => {
    const ts = this.lastModifiedAt();
    return {
      saveState: 'saved',
      savedAgo: ts === null ? null : formatRelativeTime(Date.now() - ts),
      branch: 'main',
    };
  });

  protected readonly statusRight = computed<BdStatusBarRight>(() => {
    const lines = this.focusedPaneIndex() === 0 ? this.leftLines() : this.rightLines();
    const paneCount = this.splitActive() ? 2 : 1;
    return {
      lines: lines.length,
      language: 'Markdown',
      encoding: 'UTF-8',
      cursor: { line: 1, col: 1 },
      // Pane label is appended in the bar component if needed; we surface it
      // here as the language slot when split for visibility.
    } as BdStatusBarRight & { paneCount?: number };
  });

  protected readonly topBarActions: readonly BdTopAppBarAction[] = [
    { id: 'preview', icon: 'visibility', ariaLabel: 'Toggle preview' },
    { id: 'history', icon: 'history',    ariaLabel: 'View history' },
    { id: 'share',   icon: 'share',      ariaLabel: 'Share' },
  ];

  constructor() {
    this.loadWorkspace();
    this.loadTabs();

    // Persist tab state whenever it changes — debounced 500ms.
    effect(() => {
      const panes = this.panes();
      if (!this.tabsLoaded()) return;
      const state: TabStateDto = {
        panes: panes.map(p => ({ tabs: p.tabs, activeIndex: p.activeIndex })),
      };
      this.schedulePersist(state);
    });

    // Ensure trees are fetched for every pane's active document.
    effect(() => {
      const cache = this.treeByDoc();
      const ids = new Set<number>();
      for (const p of this.panes()) {
        const id = activeDocId(p);
        if (id !== null) ids.add(id);
      }
      for (const id of ids) {
        if (!cache.has(id)) this.loadTree(id);
      }
    });
  }

  // ── Tab + pane operations ──────────────────────────────────────────────

  protected onDocumentSelected(id: number): void {
    this.openDocumentInPane(id, this.focusedPaneIndex());
  }

  protected onLeftTabSelected(idx: number): void {
    this.focusedPaneIndex.set(0);
    this.setActiveTab(0, idx);
  }

  protected onRightTabSelected(idx: number): void {
    this.focusedPaneIndex.set(1);
    this.setActiveTab(1, idx);
  }

  protected onLeftTabClosed(idx: number): void {
    this.closeTab(0, idx);
  }

  protected onRightTabClosed(idx: number): void {
    this.closeTab(1, idx);
  }

  protected onLeftPaneFocused(): void {
    this.focusedPaneIndex.set(0);
  }

  protected onRightPaneFocused(): void {
    this.focusedPaneIndex.set(1);
  }

  protected onSplitToggled(): void {
    const current = this.panes();
    if (current.length === 1) {
      const left = current[0];
      const docId = activeDocId(left);
      if (docId === null) {
        this.toast('Open a document before splitting');
        return;
      }
      this.panes.set([
        left,
        { tabs: [docId], activeIndex: 0 },
      ]);
    } else {
      this.panes.set([current[0]]);
      this.focusedPaneIndex.set(0);
    }
  }

  // ── Section / fact CRUD against the focused pane ──────────────────────

  protected onAddRootSection(): void {
    const docId = this.focusedDocId();
    if (docId === null) {
      this.toast('Open or create a document first');
      return;
    }
    this.promptForSection({ initialValue: '' }).subscribe(title => {
      if (title === null) return;
      const tree = this.treeByDoc().get(docId) ?? EMPTY_TREE;
      const position = nextPosition(tree.sections.filter(s => s.parentId === null));
      this.sectionsService.create({ documentId: docId, parentId: null, title, position }).subscribe({
        next: () => this.afterMutation(docId, 'Section created'),
        error: () => this.toast('Failed to create section'),
      });
    });
  }

  protected onSectionAction(event: SectionMenuAction, paneIdx: 0 | 1): void {
    this.focusedPaneIndex.set(paneIdx);
    switch (event.type) {
      case 'add-fact': this.onAddFact(event.sectionId); break;
      case 'add-child-section': this.onAddChildSection(event.sectionId); break;
      case 'edit-section': this.onEditSection(event.sectionId); break;
      case 'delete-section': this.onDeleteSection(event.sectionId); break;
    }
  }

  protected onFactAction(event: FactMenuAction, paneIdx: 0 | 1): void {
    this.focusedPaneIndex.set(paneIdx);
    switch (event.type) {
      case 'edit-fact': this.onEditFact(event.factId); break;
      case 'delete-fact': this.onDeleteFact(event.factId); break;
    }
  }

  protected onAddRootDocument(): void {
    this.promptForDocument({ initialValue: 'Untitled' }).subscribe(title => {
      if (title === null) return;
      const position = nextPosition(
        this.workspace().documents.filter(d => d.folderId === null),
      );
      this.documentsService.create({ folderId: null, title, position }).subscribe({
        next: created => {
          this.lastModifiedAt.set(Date.now());
          this.loadWorkspace(() => this.openDocumentInPane(created.id, this.focusedPaneIndex()));
          this.toast('Document created');
        },
        error: () => this.toast('Failed to create document'),
      });
    });
  }

  protected onAddRootFolder(): void {
    this.promptForFolder({ initialValue: 'New folder' }).subscribe(title => {
      if (title === null) return;
      const position = nextPosition(
        this.workspace().folders.filter(f => f.parentId === null),
      );
      this.foldersService.create({ parentId: null, title, position }).subscribe({
        next: () => {
          this.loadWorkspace();
          this.toast('Folder created');
        },
        error: () => this.toast('Failed to create folder'),
      });
    });
  }

  protected onOutlineClick(entry: BdOutlineEntry): void {
    const id = typeof entry.id === 'number' ? entry.id : Number(entry.id);
    if (Number.isNaN(id)) return;
    const target = document.querySelector<HTMLElement>(`[data-section-id="${id}"]`);
    if (!target) return;
    target.scrollIntoView({ behavior: 'smooth', block: 'start' });
    this.activeOutlineId.set(id);
  }

  protected onToggleNav(): void { /* TODO */ }

  protected onTopBarAction(action: BdTopAppBarAction): void { void action; }

  protected onSave(): void {
    this.lastModifiedAt.set(Date.now());
    this.toast('Saved');
  }

  // ── Section / fact helpers (driven via document-editor outputs) ───────

  private onEditSection(sectionId: number): void {
    const docId = this.focusedDocId();
    if (docId === null) return;
    const tree = this.treeByDoc().get(docId) ?? EMPTY_TREE;
    const section = tree.sections.find(s => s.id === sectionId);
    if (!section) return;
    this.promptForSection({ initialValue: section.title, title: 'Rename section' }).subscribe(title => {
      if (title === null) return;
      this.sectionsService.update(section.id, {
        parentId: section.parentId,
        title,
        position: section.position,
      }).subscribe({
        next: () => this.afterMutation(docId, 'Section renamed'),
        error: () => this.toast('Failed to rename section'),
      });
    });
  }

  private onDeleteSection(sectionId: number): void {
    const docId = this.focusedDocId();
    if (docId === null) return;
    const tree = this.treeByDoc().get(docId) ?? EMPTY_TREE;
    const section = tree.sections.find(s => s.id === sectionId);
    if (!section) return;
    this.confirm({
      title: `Delete "${section.title}"?`,
      body: 'All child sections and facts under this section will also be deleted.',
      icon: 'delete',
      tone: 'danger',
      confirmText: 'Delete',
    }).subscribe(confirmed => {
      if (!confirmed) return;
      this.sectionsService.delete(section.id).subscribe({
        next: () => this.afterMutation(docId, 'Section deleted'),
        error: () => this.toast('Failed to delete section'),
      });
    });
  }

  private onAddFact(sectionId: number): void {
    const docId = this.focusedDocId();
    if (docId === null) return;
    this.promptForFact({ initialValue: '', title: 'Add fact' }).subscribe(text => {
      if (text === null) return;
      const tree = this.treeByDoc().get(docId) ?? EMPTY_TREE;
      const position = nextPosition(tree.facts.filter(f => f.sectionId === sectionId));
      this.factsService.create({ sectionId, text, position }).subscribe({
        next: () => this.afterMutation(docId, 'Fact added'),
        error: () => this.toast('Failed to add fact'),
      });
    });
  }

  private onAddChildSection(parentId: number): void {
    const docId = this.focusedDocId();
    if (docId === null) return;
    this.promptForSection({ initialValue: '', title: 'Add child section' }).subscribe(title => {
      if (title === null) return;
      const tree = this.treeByDoc().get(docId) ?? EMPTY_TREE;
      const position = nextPosition(tree.sections.filter(s => s.parentId === parentId));
      this.sectionsService.create({ documentId: docId, parentId, title, position }).subscribe({
        next: () => this.afterMutation(docId, 'Section added'),
        error: () => this.toast('Failed to add section'),
      });
    });
  }

  private onEditFact(factId: number): void {
    const docId = this.focusedDocId();
    if (docId === null) return;
    const tree = this.treeByDoc().get(docId) ?? EMPTY_TREE;
    const fact = tree.facts.find(f => f.id === factId);
    if (!fact) return;
    this.promptForFact({ initialValue: fact.text, title: 'Edit fact' }).subscribe(text => {
      if (text === null) return;
      this.factsService.update(fact.id, {
        sectionId: fact.sectionId,
        text,
        position: fact.position,
      }).subscribe({
        next: () => this.afterMutation(docId, 'Fact updated'),
        error: () => this.toast('Failed to update fact'),
      });
    });
  }

  private onDeleteFact(factId: number): void {
    const docId = this.focusedDocId();
    if (docId === null) return;
    const tree = this.treeByDoc().get(docId) ?? EMPTY_TREE;
    const fact = tree.facts.find(f => f.id === factId);
    if (!fact) return;
    this.confirm({
      title: 'Delete fact?',
      body: fact.text.slice(0, 120),
      icon: 'delete',
      tone: 'danger',
      confirmText: 'Delete',
    }).subscribe(confirmed => {
      if (!confirmed) return;
      this.factsService.delete(fact.id).subscribe({
        next: () => this.afterMutation(docId, 'Fact deleted'),
        error: () => this.toast('Failed to delete fact'),
      });
    });
  }

  // ── Tab state plumbing ────────────────────────────────────────────────

  private tabsFor(p: Pane): readonly BdTab[] {
    const docs = this.workspace().documents;
    return p.tabs
      .map(id => {
        const doc = docs.find(d => d.id === id);
        return doc ? { id, title: doc.title } : null;
      })
      .filter((t): t is BdTab => t !== null);
  }

  private linesFor(docId: number | null): readonly BdEditorLine[] {
    if (docId === null) return [];
    const tree = this.treeByDoc().get(docId);
    if (!tree) return [];
    return buildLines(tree);
  }

  private openDocumentInPane(docId: number, paneIdx: number): void {
    const panes = [...this.panes()];
    const pane = panes[paneIdx] ?? panes[0];
    const targetIdx = panes[paneIdx] ? paneIdx : 0;
    const existing = pane.tabs.indexOf(docId);
    if (existing >= 0) {
      panes[targetIdx] = { ...pane, activeIndex: existing };
    } else {
      panes[targetIdx] = { tabs: [...pane.tabs, docId], activeIndex: pane.tabs.length };
    }
    this.panes.set(panes);
  }

  private setActiveTab(paneIdx: number, tabIdx: number): void {
    const panes = [...this.panes()];
    const pane = panes[paneIdx];
    if (!pane) return;
    panes[paneIdx] = { ...pane, activeIndex: tabIdx };
    this.panes.set(panes);
  }

  private closeTab(paneIdx: number, tabIdx: number): void {
    const panes = [...this.panes()];
    const pane = panes[paneIdx];
    if (!pane) return;
    const newTabs = pane.tabs.filter((_, i) => i !== tabIdx);
    if (newTabs.length === 0 && paneIdx === 1) {
      // Right pane empty — collapse split.
      panes.splice(1, 1);
      this.panes.set(panes);
      this.focusedPaneIndex.set(0);
      return;
    }
    let newActive = pane.activeIndex;
    if (newTabs.length === 0) {
      newActive = -1;
    } else if (tabIdx < pane.activeIndex) {
      newActive = pane.activeIndex - 1;
    } else if (tabIdx === pane.activeIndex) {
      newActive = Math.max(0, pane.activeIndex - 1);
    }
    panes[paneIdx] = { tabs: newTabs, activeIndex: newActive };
    this.panes.set(panes);
  }

  // ── Loaders ───────────────────────────────────────────────────────────

  private loadWorkspace(after?: () => void): void {
    this.loading.set(true);
    this.workspaceService.get().subscribe({
      next: ws => {
        this.workspace.set(ws);
        this.loading.set(false);
        // If panes are still empty after the initial GET /api/tabs has
        // settled, default the first pane to the first document.
        if (this.tabsLoaded() && this.leftPane().tabs.length === 0 && ws.documents.length > 0) {
          this.openDocumentInPane(ws.documents[0].id, 0);
        }
        after?.();
      },
      error: () => {
        this.loading.set(false);
        this.toast('Failed to load workspace');
      },
    });
  }

  private loadTabs(): void {
    this.tabsService.get().subscribe({
      next: state => {
        if (state.panes.length > 0) {
          this.panes.set(state.panes.map(p => ({ tabs: [...p.tabs], activeIndex: p.activeIndex })));
        }
        this.tabsLoaded.set(true);
      },
      error: () => {
        // Treat as fresh state.
        this.tabsLoaded.set(true);
      },
    });
  }

  private loadTree(docId: number): void {
    this.documentsService.getTree(docId).subscribe({
      next: tree => {
        const next = new Map(this.treeByDoc());
        next.set(docId, tree);
        this.treeByDoc.set(next);
      },
      error: () => this.toast('Failed to load document tree'),
    });
  }

  private invalidateTree(docId: number): void {
    const next = new Map(this.treeByDoc());
    next.delete(docId);
    this.treeByDoc.set(next);
    this.loadTree(docId);
  }

  private afterMutation(docId: number, message: string): void {
    this.lastModifiedAt.set(Date.now());
    this.invalidateTree(docId);
    this.toast(message);
  }

  private schedulePersist(state: TabStateDto): void {
    if (this.persistTimer) clearTimeout(this.persistTimer);
    this.persistTimer = setTimeout(() => {
      this.tabsService.put(state).subscribe();
      this.persistTimer = null;
    }, 500);
  }

  // ── Dialog helpers ────────────────────────────────────────────────────

  private promptForSection(opts: { initialValue: string; title?: string }): Observable<string | null> {
    return this.openPrompt({
      title: opts.title ?? 'Add section',
      label: 'Title',
      placeholder: 'Section title',
      initialValue: opts.initialValue,
    });
  }

  private promptForFact(opts: { initialValue: string; title?: string }): Observable<string | null> {
    return this.openPrompt({
      title: opts.title ?? 'Edit fact',
      label: 'Fact',
      placeholder: 'Declarative fact text',
      initialValue: opts.initialValue,
      multiline: true,
    });
  }

  private promptForDocument(opts: { initialValue: string; title?: string }): Observable<string | null> {
    return this.openPrompt({
      title: opts.title ?? 'New document',
      label: 'Title',
      placeholder: 'Document title',
      initialValue: opts.initialValue,
    });
  }

  private promptForFolder(opts: { initialValue: string; title?: string }): Observable<string | null> {
    return this.openPrompt({
      title: opts.title ?? 'New folder',
      label: 'Title',
      placeholder: 'Folder title',
      initialValue: opts.initialValue,
    });
  }

  private openPrompt(data: BdPromptDialogData): Observable<string | null> {
    return this.dialog
      .open<BdPromptDialog, BdPromptDialogData, string | null>(BdPromptDialog, {
        data,
        panelClass: 'bd-dialog-panel',
        autoFocus: 'first-tabbable',
      })
      .afterClosed() as Observable<string | null>;
  }

  private confirm(data: BdConfirmDialogData): Observable<boolean> {
    return this.dialog
      .open<BdConfirmDialog, BdConfirmDialogData, boolean>(BdConfirmDialog, {
        data,
        panelClass: 'bd-dialog-panel',
      })
      .afterClosed() as Observable<boolean>;
  }

  private toast(message: string): void {
    this.snackbar.openFromComponent(BdSnackbar, {
      data: { message },
      duration: 3000,
    });
  }
}

function activeDocId(p: Pane): number | null {
  if (p.activeIndex < 0 || p.activeIndex >= p.tabs.length) return null;
  return p.tabs[p.activeIndex];
}

function buildLines(tree: TreeDto): BdEditorLine[] {
  const out: BdEditorLine[] = [];
  let lineNo = 1;
  const sectionsByParent = new Map<number | null, SectionDto[]>();
  for (const s of tree.sections) {
    const list = sectionsByParent.get(s.parentId) ?? [];
    list.push(s);
    sectionsByParent.set(s.parentId, list);
  }
  for (const list of sectionsByParent.values()) list.sort((a, b) => a.position - b.position);

  const factsBySection = new Map<number, FactDto[]>();
  for (const f of tree.facts) {
    const list = factsBySection.get(f.sectionId) ?? [];
    list.push(f);
    factsBySection.set(f.sectionId, list);
  }
  for (const list of factsBySection.values()) list.sort((a, b) => a.position - b.position);

  const walk = (section: SectionDto, depth: number): void => {
    const prefix = '#'.repeat(depth + 1);
    const level: 1 | 2 | 3 = depth === 0 ? 1 : depth === 1 ? 2 : 3;
    out.push({
      lineNumber: lineNo++,
      code: `${prefix} ${section.title}`,
      kind: 'section',
      sectionId: section.id,
      depth,
      level,
    });
    for (const fact of factsBySection.get(section.id) ?? []) {
      out.push({
        lineNumber: lineNo++,
        code: `- ${fact.text}`,
        kind: 'fact',
        factId: fact.id,
        sectionId: section.id,
        depth,
      });
    }
    for (const child of sectionsByParent.get(section.id) ?? []) {
      walk(child, depth + 1);
    }
    out.push({ lineNumber: lineNo++, code: '', kind: 'blank' });
  };

  for (const root of sectionsByParent.get(null) ?? []) walk(root, 0);
  return out;
}

function clampLevel(n: number): 1 | 2 | 3 {
  if (n <= 1) return 1;
  if (n >= 3) return 3;
  return 2;
}

function formatRelativeTime(diffMs: number): string {
  const minutes = Math.floor(diffMs / 60_000);
  if (minutes < 1) return 'just now';
  if (minutes === 1) return '1 minute ago';
  if (minutes < 60) return `${minutes} minutes ago`;
  const hours = Math.floor(minutes / 60);
  if (hours === 1) return '1 hour ago';
  if (hours < 24) return `${hours} hours ago`;
  const days = Math.floor(hours / 24);
  if (days === 1) return 'yesterday';
  if (days < 7) return `${days} days ago`;
  const weeks = Math.floor(days / 7);
  return weeks === 1 ? '1 week ago' : `${weeks} weeks ago`;
}

function nextPosition(items: readonly { position: number }[]): number {
  if (items.length === 0) return 10;
  return Math.max(...items.map(i => i.position)) + 10;
}
