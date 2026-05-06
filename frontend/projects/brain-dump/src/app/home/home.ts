import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatMenu, MatMenuContent, MatMenuItem, MatMenuTrigger } from '@angular/material/menu';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatIcon } from '@angular/material/icon';
import { Observable } from 'rxjs';
import {
  DOCUMENTS_SERVICE,
  DocumentDto,
  FACTS_SERVICE,
  FactDto,
  FOLDERS_SERVICE,
  FolderDto,
  SECTIONS_SERVICE,
  SectionDto,
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
  BdIconButton,
  BdMonacoLine,
  BdNavItem,
  BdOutline,
  BdOutlineEntry,
  BdPageTree,
  BdPromptDialog,
  BdPromptDialogData,
  BdSideRail,
  BdSidebar,
  BdSnackbar,
  BdStatusBar,
  BdStatusBarLeft,
  BdStatusBarRight,
  BdTextField,
  BdTopAppBar,
  BdTopAppBarAction,
} from 'components';

interface RenderedLine {
  readonly lineNumber: number;
  readonly code: string;
  readonly kind: 'section' | 'fact' | 'blank';
  readonly sectionId?: number;
  readonly factId?: number;
  readonly depth?: number;
  readonly level?: 0 | 1 | 2 | 3;
}

@Component({
  selector: 'app-home',
  imports: [
    MatIcon,
    MatMenu,
    MatMenuContent,
    MatMenuItem,
    MatMenuTrigger,
    BdTopAppBar,
    BdSideRail,
    BdSidebar,
    BdNavItem,
    BdMonacoLine,
    BdIconButton,
    BdButton,
    BdTextField,
    BdChip,
    BdOutline,
    BdBacklinks,
    BdStatusBar,
    BdPageTree,
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
  private readonly dialog = inject(MatDialog);
  private readonly snackbar = inject(MatSnackBar);

  protected readonly workspace = signal<WorkspaceDto>({ folders: [], documents: [] });
  protected readonly tree = signal<TreeDto>({ sections: [], facts: [] });
  protected readonly activeDocumentId = signal<number | null>(null);
  protected readonly loading = signal(true);
  protected readonly searchQuery = signal('');
  protected readonly filter = signal<'all' | 'facts' | 'wip'>('all');
  protected readonly lastModifiedAt = signal<number | null>(null);

  protected readonly activeDocument = computed<DocumentDto | null>(() => {
    const id = this.activeDocumentId();
    if (id === null) return null;
    return this.workspace().documents.find(d => d.id === id) ?? null;
  });

  protected readonly editorTitle = computed(() => this.activeDocument()?.title ?? '—');

  protected readonly lastEditedCaption = computed(() => {
    const ts = this.lastModifiedAt();
    if (ts === null) return null;
    return `Edited ${formatRelativeTime(Date.now() - ts)}`;
  });

  protected readonly outlineEntries = computed<readonly BdOutlineEntry[]>(() => {
    return this.lines()
      .filter(l => l.kind === 'section' && l.sectionId !== undefined)
      .map<BdOutlineEntry>(l => ({
        id: l.sectionId!,
        label: this.tree().sections.find(s => s.id === l.sectionId)?.title ?? '',
        level: clampLevel((l.depth ?? 0) + 1),
      }));
  });

  protected readonly activeOutlineId = signal<number | null>(null);
  // TODO: derive from documents that link to the active document once
  // L1-019 (Cross-Document Backlinks) lands.
  protected readonly backlinks = signal<readonly BdBacklinkEntry[]>([]);

  protected readonly statusLeft = computed<BdStatusBarLeft>(() => {
    const ts = this.lastModifiedAt();
    return {
      saveState: 'saved',
      savedAgo: ts === null ? null : formatRelativeTime(Date.now() - ts),
      branch: 'main',
    };
  });

  protected readonly statusRight = computed<BdStatusBarRight>(() => ({
    lines: this.lines().length,
    language: 'Markdown',
    encoding: 'UTF-8',
    cursor: { line: 1, col: 1 },
  }));

  protected readonly topBarActions: readonly BdTopAppBarAction[] = [
    { id: 'preview', icon: 'visibility', ariaLabel: 'Toggle preview' },
    { id: 'history', icon: 'history',    ariaLabel: 'View history' },
    { id: 'share',   icon: 'share',      ariaLabel: 'Share' },
  ];

  protected readonly lines = computed<RenderedLine[]>(() => {
    const t = this.tree();
    const out: RenderedLine[] = [];
    let lineNo = 1;

    const sectionsByParent = new Map<number | null, SectionDto[]>();
    for (const s of t.sections) {
      const list = sectionsByParent.get(s.parentId) ?? [];
      list.push(s);
      sectionsByParent.set(s.parentId, list);
    }
    for (const list of sectionsByParent.values()) {
      list.sort((a, b) => a.position - b.position);
    }

    const factsBySection = new Map<number, FactDto[]>();
    for (const f of t.facts) {
      const list = factsBySection.get(f.sectionId) ?? [];
      list.push(f);
      factsBySection.set(f.sectionId, list);
    }
    for (const list of factsBySection.values()) {
      list.sort((a, b) => a.position - b.position);
    }

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

    for (const root of sectionsByParent.get(null) ?? []) {
      walk(root, 0);
    }

    return out;
  });

  constructor() {
    this.loadWorkspace();

    // Whenever the active document changes, fetch its tree.
    effect(() => {
      const id = this.activeDocumentId();
      if (id === null) {
        this.tree.set({ sections: [], facts: [] });
        return;
      }
      this.documentsService.getTree(id).subscribe({
        next: tree => this.tree.set(tree),
        error: () => this.toast('Failed to load document tree'),
      });
    });
  }

  protected onDocumentSelected(id: number): void {
    this.activeDocumentId.set(id);
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
          this.loadWorkspace(() => this.activeDocumentId.set(created.id));
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

  protected onToggleNav(): void {
    // TODO: wire to layout state once a nav-collapsed signal exists
  }

  protected onTopBarAction(action: BdTopAppBarAction): void {
    void action;
  }

  protected onSave(): void {
    this.lastModifiedAt.set(Date.now());
    this.toast('Saved');
  }

  protected onAddRootSection(): void {
    const docId = this.activeDocumentId();
    if (docId === null) {
      this.toast('Open or create a document first');
      return;
    }
    this.promptForSection({ initialValue: '' }).subscribe(title => {
      if (title === null) return;
      const position = nextPosition(
        this.tree().sections.filter(s => s.parentId === null)
      );
      this.sectionsService.create({ documentId: docId, parentId: null, title, position }).subscribe({
        next: () => {
          this.lastModifiedAt.set(Date.now());
          this.refreshTree();
          this.toast('Section created');
        },
        error: () => this.toast('Failed to create section'),
      });
    });
  }

  protected onEditSection(sectionId: number): void {
    const section = this.tree().sections.find(s => s.id === sectionId);
    if (!section) return;
    this.promptForSection({ initialValue: section.title, title: 'Rename section' }).subscribe(title => {
      if (title === null) return;
      this.sectionsService.update(section.id, {
        parentId: section.parentId,
        title,
        position: section.position,
      }).subscribe({
        next: () => {
          this.lastModifiedAt.set(Date.now());
          this.refreshTree();
          this.toast('Section renamed');
        },
        error: () => this.toast('Failed to rename section'),
      });
    });
  }

  protected onDeleteSection(sectionId: number): void {
    const section = this.tree().sections.find(s => s.id === sectionId);
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
        next: () => {
          this.lastModifiedAt.set(Date.now());
          this.refreshTree();
          this.toast('Section deleted');
        },
        error: () => this.toast('Failed to delete section'),
      });
    });
  }

  protected onAddFact(sectionId: number): void {
    this.promptForFact({ initialValue: '', title: 'Add fact' }).subscribe(text => {
      if (text === null) return;
      const position = nextPosition(this.tree().facts.filter(f => f.sectionId === sectionId));
      this.factsService.create({ sectionId, text, position }).subscribe({
        next: () => {
          this.lastModifiedAt.set(Date.now());
          this.refreshTree();
          this.toast('Fact added');
        },
        error: () => this.toast('Failed to add fact'),
      });
    });
  }

  protected onAddChildSection(parentId: number): void {
    const docId = this.activeDocumentId();
    if (docId === null) return;
    this.promptForSection({ initialValue: '', title: 'Add child section' }).subscribe(title => {
      if (title === null) return;
      const position = nextPosition(this.tree().sections.filter(s => s.parentId === parentId));
      this.sectionsService.create({ documentId: docId, parentId, title, position }).subscribe({
        next: () => {
          this.lastModifiedAt.set(Date.now());
          this.refreshTree();
          this.toast('Section added');
        },
        error: () => this.toast('Failed to add section'),
      });
    });
  }

  protected onEditFact(factId: number): void {
    const fact = this.tree().facts.find(f => f.id === factId);
    if (!fact) return;
    this.promptForFact({ initialValue: fact.text, title: 'Edit fact' }).subscribe(text => {
      if (text === null) return;
      this.factsService.update(fact.id, {
        sectionId: fact.sectionId,
        text,
        position: fact.position,
      }).subscribe({
        next: () => {
          this.lastModifiedAt.set(Date.now());
          this.refreshTree();
          this.toast('Fact updated');
        },
        error: () => this.toast('Failed to update fact'),
      });
    });
  }

  protected onDeleteFact(factId: number): void {
    const fact = this.tree().facts.find(f => f.id === factId);
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
        next: () => {
          this.lastModifiedAt.set(Date.now());
          this.refreshTree();
          this.toast('Fact deleted');
        },
        error: () => this.toast('Failed to delete fact'),
      });
    });
  }

  private loadWorkspace(after?: () => void): void {
    this.loading.set(true);
    this.workspaceService.get().subscribe({
      next: ws => {
        this.workspace.set(ws);
        this.loading.set(false);
        // If no active doc selected yet, default to the first document so
        // the editor has something to render.
        if (this.activeDocumentId() === null && ws.documents.length > 0) {
          this.activeDocumentId.set(ws.documents[0].id);
        }
        after?.();
      },
      error: () => {
        this.loading.set(false);
        this.toast('Failed to load workspace');
      },
    });
  }

  private refreshTree(): void {
    const id = this.activeDocumentId();
    if (id === null) return;
    this.documentsService.getTree(id).subscribe({
      next: tree => this.tree.set(tree),
      error: () => this.toast('Failed to refresh document'),
    });
  }

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
