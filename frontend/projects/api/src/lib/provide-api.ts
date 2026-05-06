import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { API_BASE_URL } from './api-base-url';
import { AuthService } from './auth/auth.service';
import { AUTH_SERVICE } from './auth/auth.service.contract';
import { DocumentsService } from './documents/documents.service';
import { DOCUMENTS_SERVICE } from './documents/documents.service.contract';
import { FactsService } from './facts/facts.service';
import { FACTS_SERVICE } from './facts/facts.service.contract';
import { FoldersService } from './folders/folders.service';
import { FOLDERS_SERVICE } from './folders/folders.service.contract';
import { MoveServiceImpl } from './move/move.service';
import { MOVE_SERVICE } from './move/move.service.contract';
import { ReorderService } from './reorder/reorder.service';
import { REORDER_SERVICE } from './reorder/reorder.service.contract';
import { SectionsService } from './sections/sections.service';
import { SECTIONS_SERVICE } from './sections/sections.service.contract';
import { TabsService } from './tabs/tabs.service';
import { TABS_SERVICE } from './tabs/tabs.service.contract';
import { WorkspaceService } from './workspace/workspace.service';
import { WORKSPACE_SERVICE } from './workspace/workspace.service.contract';

export interface ApiConfig {
  baseUrl?: string;
}

export function provideApi(config: ApiConfig = {}): EnvironmentProviders {
  return makeEnvironmentProviders([
    { provide: API_BASE_URL, useValue: config.baseUrl ?? '' },
    { provide: AUTH_SERVICE, useExisting: AuthService },
    { provide: WORKSPACE_SERVICE, useExisting: WorkspaceService },
    { provide: DOCUMENTS_SERVICE, useExisting: DocumentsService },
    { provide: FOLDERS_SERVICE, useExisting: FoldersService },
    { provide: MOVE_SERVICE, useExisting: MoveServiceImpl },
    { provide: SECTIONS_SERVICE, useExisting: SectionsService },
    { provide: TABS_SERVICE, useExisting: TabsService },
    { provide: FACTS_SERVICE, useExisting: FactsService },
    { provide: REORDER_SERVICE, useExisting: ReorderService },
  ]);
}
