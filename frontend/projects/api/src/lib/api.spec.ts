import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Api } from './api';

describe('Api', () => {
  let component: Api;
  let fixture: ComponentFixture<Api>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Api],
    }).compileComponents();

    fixture = TestBed.createComponent(Api);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
