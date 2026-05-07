export interface DocumentDto {
  readonly id: number;
  readonly folderId: number | null;
  readonly title: string;
  readonly position: number;
  readonly createdAt: string;
  readonly updatedAt: string;
  readonly labels: readonly string[];
}
