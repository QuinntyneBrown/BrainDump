import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { FactsService } from './facts/facts.service';
import { FACTS_SERVICE } from './facts/facts.service.contract';
import { ReorderService } from './reorder/reorder.service';
import { REORDER_SERVICE } from './reorder/reorder.service.contract';
import { SectionsService } from './sections/sections.service';
import { SECTIONS_SERVICE } from './sections/sections.service.contract';
import { TreeService } from './tree/tree.service';
import { TREE_SERVICE } from './tree/tree.service.contract';

export function provideApi(): EnvironmentProviders {
  return makeEnvironmentProviders([
    { provide: TREE_SERVICE, useExisting: TreeService },
    { provide: SECTIONS_SERVICE, useExisting: SectionsService },
    { provide: FACTS_SERVICE, useExisting: FactsService },
    { provide: REORDER_SERVICE, useExisting: ReorderService },
  ]);
}
