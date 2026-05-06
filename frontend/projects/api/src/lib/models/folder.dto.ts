export interface FolderDto {
  readonly id: number;
  readonly parentId: number | null;
  readonly title: string;
  readonly position: number;
}
