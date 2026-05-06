import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatMenu, MatMenuContent, MatMenuItem, MatMenuTrigger } from '@angular/material/menu';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatIcon } from '@angular/material/icon';
import { Observable } from 'rxjs';
import {
  AUTH_SERVICE,
  FACTS_SERVICE,
  FactDto,
  SECTIONS_SERVICE,
  SectionDto,
  TREE_SERVICE,
  TreeDto,
} from 'api';
import {
  BdButton,
  BdConfirmDialog,
  BdConfirmDialogData,
  BdFab,
  BdIconButton,
  BdMonacoLine,
  BdNavItem,
  BdNoteItem,
  BdPromptDialog,
  BdPromptDialogData,
  BdSideRail,
  BdSidebar,
  BdSnackbar,
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
}

interface SectionSummary {
  readonly section: SectionDto;
  readonly factCount: number;
  readonly preview: string;
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
  ],
  templateUrl: './home.html',
  styleUrl: './home.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Home {
  private readonly authService = inject(AUTH_SERVICE);
  private readonly treeService = inject(TREE_SERVICE);
  private readonly sectionsService = inject(SECTIONS_SERVICE);
  private readonly factsService = inject(FACTS_SERVICE);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackbar = inject(MatSnackBar);

  protected readonly tree = signal<TreeDto>({ sections: [], facts: [] });
  protected readonly loading = signal(true);

  protected readonly topBarActions: readonly BdTopAppBarAction[] = [
    { id: 'search', icon: 'search', ariaLabel: 'Search notes' },
    { id: 'sign-out', icon: 'logout', ariaLabel: 'Sign out' },
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
        const descendantIds = collectDescendantIds(t.sections, section.id);
        let totalFacts = directFacts.length;
        for (const id of descendantIds) {
          totalFacts += (factsBySection.get(id) ?? []).length;
        }
        const preview = directFacts.length > 0
          ? directFacts[0].text.slice(0, 80)
          : 'Empty section';
        return { section, factCount: totalFacts, preview };
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
      out.push({
        lineNumber: lineNo++,
        code: `${prefix} ${section.title}`,
        kind: 'section',
        sectionId: section.id,
        depth,
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

  protected onToggleNav(): void {
    // TODO: wire to layout state once a nav-collapsed signal exists
  }

  protected onTopBarAction(action: BdTopAppBarAction): void {
    if (action.id === 'sign-out') {
      this.signOut();
    }
  }

  protected onAddRootSection(): void {
    this.promptForSection({ initialValue: '' }).subscribe(title => {
      if (title === null) return;
      const position = nextPosition(
        this.tree().sections.filter(s => s.parentId === null)
      );
      this.sectionsService.create({ parentId: null, title, position }).subscribe({
        next: () => {
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
          this.loadTree();
          this.toast('Fact deleted');
        },
        error: () => this.toast('Failed to delete fact'),
      });
    });
  }

  protected signOut(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
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

function collectDescendantIds(sections: readonly SectionDto[], rootId: number): number[] {
  const result: number[] = [];
  const queue: number[] = [rootId];
  while (queue.length > 0) {
    const current = queue.shift()!;
    for (const s of sections) {
      if (s.parentId === current) {
        result.push(s.id);
        queue.push(s.id);
      }
    }
  }
  return result;
}

function nextPosition(items: readonly { position: number }[]): number {
  if (items.length === 0) return 10;
  return Math.max(...items.map(i => i.position)) + 10;
}
