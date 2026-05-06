import { FactDto } from './fact.dto';
import { SectionDto } from './section.dto';

export interface TreeDto {
  sections: SectionDto[];
  facts: FactDto[];
}
