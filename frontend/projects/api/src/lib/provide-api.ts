import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { API_BASE_URL } from './api-base-url';
import { AuthService } from './auth/auth.service';
import { AUTH_SERVICE } from './auth/auth.service.contract';
import { FactsService } from './facts/facts.service';
import { FACTS_SERVICE } from './facts/facts.service.contract';
import { ReorderService } from './reorder/reorder.service';
import { REORDER_SERVICE } from './reorder/reorder.service.contract';
import { SectionsService } from './sections/sections.service';
import { SECTIONS_SERVICE } from './sections/sections.service.contract';
import { TreeService } from './tree/tree.service';
import { TREE_SERVICE } from './tree/tree.service.contract';

export interface ApiConfig {
  baseUrl?: string;
}

export function provideApi(config: ApiConfig = {}): EnvironmentProviders {
  return makeEnvironmentProviders([
    { provide: API_BASE_URL, useValue: config.baseUrl ?? '' },
    { provide: AUTH_SERVICE, useExisting: AuthService },
    { provide: TREE_SERVICE, useExisting: TreeService },
    { provide: SECTIONS_SERVICE, useExisting: SectionsService },
    { provide: FACTS_SERVICE, useExisting: FactsService },
    { provide: REORDER_SERVICE, useExisting: ReorderService },
  ]);
}
