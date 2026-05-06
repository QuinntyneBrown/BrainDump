import { DocumentDto } from './document.dto';
import { FolderDto } from './folder.dto';

export interface WorkspaceDto {
  readonly folders: readonly FolderDto[];
  readonly documents: readonly DocumentDto[];
}
