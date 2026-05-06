export interface TabPaneDto {
  readonly tabs: readonly number[];
  readonly activeIndex: number;
}

export interface TabStateDto {
  readonly panes: readonly TabPaneDto[];
}
