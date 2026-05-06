import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { MatIcon } from '@angular/material/icon';

export interface BdPageTreeFolder {
  readonly id: number;
  readonly parentId: number | null;
  readonly title: string;
  readonly position: number;
}

export interface BdPageTreeDocument {
  readonly id: number;
  readonly folderId: number | null;
  readonly title: string;
  readonly position: number;
}

interface FolderNode {
  readonly kind: 'folder';
  readonly id: number;
  readonly title: string;
  readonly depth: number;
  readonly children: readonly TreeNode[];
}

interface DocumentNode {
  readonly kind: 'document';
  readonly id: number;
  readonly title: string;
  readonly depth: number;
}

type TreeNode = FolderNode | DocumentNode;

@Component({
  selector: 'bd-page-tree',
  imports: [MatIcon],
  templateUrl: './page-tree.html',
  styleUrl: './page-tree.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BdPageTree {
  readonly folders = input<readonly BdPageTreeFolder[]>([]);
  readonly documents = input<readonly BdPageTreeDocument[]>([]);
  readonly activeDocumentId = input<number | null>(null);

  readonly documentSelected = output<number>();

  protected readonly collapsed = signal<ReadonlySet<number>>(new Set());

  protected readonly nodes = computed<readonly TreeNode[]>(() => {
    const folders = [...this.folders()].sort((a, b) => a.position - b.position);
    const documents = [...this.documents()].sort((a, b) => a.position - b.position);

    const foldersByParent = new Map<number | null, BdPageTreeFolder[]>();
    for (const f of folders) {
      const list = foldersByParent.get(f.parentId) ?? [];
      list.push(f);
      foldersByParent.set(f.parentId, list);
    }

    const docsByFolder = new Map<number | null, BdPageTreeDocument[]>();
    for (const d of documents) {
      const list = docsByFolder.get(d.folderId) ?? [];
      list.push(d);
      docsByFolder.set(d.folderId, list);
    }

    const collapsed = this.collapsed();

    const buildFolder = (folder: BdPageTreeFolder, depth: number): FolderNode => {
      const childFolders = (foldersByParent.get(folder.id) ?? []).map(f => buildFolder(f, depth + 1));
      const childDocs = (docsByFolder.get(folder.id) ?? []).map<DocumentNode>(d => ({
        kind: 'document',
        id: d.id,
        title: d.title,
        depth: depth + 1,
      }));
      const children = collapsed.has(folder.id) ? [] : [...childFolders, ...childDocs];
      return { kind: 'folder', id: folder.id, title: folder.title, depth, children };
    };

    const flatten = (n: TreeNode, out: TreeNode[]): void => {
      out.push(n);
      if (n.kind === 'folder') {
        for (const child of n.children) flatten(child, out);
      }
    };

    const result: TreeNode[] = [];
    for (const f of foldersByParent.get(null) ?? []) {
      flatten(buildFolder(f, 0), result);
    }
    for (const d of docsByFolder.get(null) ?? []) {
      result.push({ kind: 'document', id: d.id, title: d.title, depth: 0 });
    }
    return result;
  });

  protected isFolder(node: TreeNode): node is FolderNode {
    return node.kind === 'folder';
  }

  protected onFolderClick(id: number): void {
    const next = new Set(this.collapsed());
    if (next.has(id)) next.delete(id); else next.add(id);
    this.collapsed.set(next);
  }

  protected onDocumentClick(id: number): void {
    this.documentSelected.emit(id);
  }

  protected isCollapsed(id: number): boolean {
    return this.collapsed().has(id);
  }

  protected indentPadding(depth: number): string {
    return `${4 + depth * 16}px`;
  }
}
