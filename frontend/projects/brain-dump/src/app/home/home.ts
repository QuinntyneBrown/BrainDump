import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatMenu, MatMenuContent, MatMenuItem, MatMenuTrigger } from '@angular/material/menu';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatIcon } from '@angular/material/icon';
import { Observable } from 'rxjs';
import {
  FACTS_SERVICE,
  FactDto,
  SECTIONS_SERVICE,
  SectionDto,
  TREE_SERVICE,
  TreeDto,
} from 'api';
import {
  BdBacklinkEntry,
  BdBacklinks,
  BdButton,
  BdChip,
  BdConfirmDialog,
  BdConfirmDialogData,
  BdFab,
  BdIconButton,
  BdMonacoLine,
  BdNavItem,
  BdNoteItem,
  BdOutline,
  BdOutlineEntry,
  BdPromptDialog,
  BdPromptDialogData,
  BdSideRail,
  BdSidebar,
  BdSnackbar,
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

interface SectionSummary {
  readonly section: SectionDto;
  readonly preview: string;
  readonly meta: string | null;
  readonly tags: readonly string[];
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
    BdNoteItem,
    BdMonacoLine,
    BdFab,
    BdIconButton,
    BdButton,
    BdTextField,
    BdChip,
    BdOutline,
    BdBacklinks,
  ],
  templateUrl: './home.html',
  styleUrl: './home.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Home {
  private readonly treeService = inject(TREE_SERVICE);
  private readonly sectionsService = inject(SECTIONS_SERVICE);
  private readonly factsService = inject(FACTS_SERVICE);
  private readonly dialog = inject(MatDialog);
  private readonly snackbar = inject(MatSnackBar);

  protected readonly tree = signal<TreeDto>({ sections: [], facts: [] });
  protected readonly loading = signal(true);
  protected readonly searchQuery = signal('');
  protected readonly filter = signal<'all' | 'facts' | 'wip'>('all');
  protected readonly lastModifiedAt = signal<number | null>(null);

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
  // TODO: derive from documents that link to brain-dump.md once that data
  // model exists. Stubbed empty for now per task 16 acceptance criteria.
  protected readonly backlinks = signal<readonly BdBacklinkEntry[]>([]);

  protected readonly topBarActions: readonly BdTopAppBarAction[] = [
    { id: 'preview', icon: 'visibility', ariaLabel: 'Toggle preview' },
    { id: 'history', icon: 'history',    ariaLabel: 'View history' },
    { id: 'share',   icon: 'share',      ariaLabel: 'Share' },
  ];

  protected readonly rootSections = computed<SectionSummary[]>(() => {
    const t = this.tree();
    const factsBySection = new Map<number, FactDto[]>();
    for (const f of t.facts) {
      const list = factsBySection.get(f.sectionId) ?? [];
      list.push(f);
      factsBySection.set(f.sectionId, list);
    }
    return t.sections
      .filter(s => s.parentId === null)
      .sort((a, b) => a.position - b.position)
      .map<SectionSummary>(section => {
        const directFacts = factsBySection.get(section.id) ?? [];
        const preview = directFacts.length > 0
          ? directFacts[0].text.slice(0, 80)
          : 'Empty section';
        // SectionDto does not carry timestamps yet — meta stays null until
        // updatedAt lands on the backend (see task 09 backlog).
        return { section, preview, meta: null, tags: [] };
      });
  });

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
    this.loadTree();
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
    // TODO: wire preview / history / share once the matching surfaces exist.
    // Sign-out has moved out of the toolbar; surface it from the avatar menu
    // (rail) when that menu is wired up.
    void action;
  }

  protected onSave(): void {
    this.lastModifiedAt.set(Date.now());
    this.toast('Saved');
  }

  protected onAddRootSection(): void {
    this.promptForSection({ initialValue: '' }).subscribe(title => {
      if (title === null) return;
      const position = nextPosition(
        this.tree().sections.filter(s => s.parentId === null)
      );
      this.sectionsService.create({ parentId: null, title, position }).subscribe({
        next: () => {
          this.lastModifiedAt.set(Date.now());
          this.loadTree();
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
          this.loadTree();
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
          this.loadTree();
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
          this.loadTree();
          this.toast('Fact added');
        },
        error: () => this.toast('Failed to add fact'),
      });
    });
  }

  protected onAddChildSection(parentId: number): void {
    this.promptForSection({ initialValue: '', title: 'Add child section' }).subscribe(title => {
      if (title === null) return;
      const position = nextPosition(this.tree().sections.filter(s => s.parentId === parentId));
      this.sectionsService.create({ parentId, title, position }).subscribe({
        next: () => {
          this.lastModifiedAt.set(Date.now());
          this.loadTree();
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
          this.loadTree();
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
          this.loadTree();
          this.toast('Fact deleted');
        },
        error: () => this.toast('Failed to delete fact'),
      });
    });
  }

  private loadTree(): void {
    this.loading.set(true);
    this.treeService.getTree().subscribe({
      next: tree => {
        this.tree.set(tree);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.toast('Failed to load tree');
      },
    });
  }

  private promptForSection(opts: { initialValue: string; title?: string }): Observable<string | null> {
    const data: BdPromptDialogData = {
      title: opts.title ?? 'Add section',
      label: 'Title',
      placeholder: 'Section title',
      initialValue: opts.initialValue,
    };
    return this.dialog
      .open<BdPromptDialog, BdPromptDialogData, string | null>(BdPromptDialog, {
        data,
        panelClass: 'bd-dialog-panel',
        autoFocus: 'first-tabbable',
      })
      .afterClosed() as Observable<string | null>;
  }

  private promptForFact(opts: { initialValue: string; title?: string }): Observable<string | null> {
    const data: BdPromptDialogData = {
      title: opts.title ?? 'Edit fact',
      label: 'Fact',
      placeholder: 'Declarative fact text',
      initialValue: opts.initialValue,
      multiline: true,
    };
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
