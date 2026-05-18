import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SlaStateBadgeComponent } from './sla-state-badge.component';

describe('SlaStateBadgeComponent', () => {
  let fixture: ComponentFixture<SlaStateBadgeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SlaStateBadgeComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(SlaStateBadgeComponent);
  });

  it('renders SLA labels and classes', () => {
    fixture.componentInstance.slaState = 'AtRisk';
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('.sla-badge') as HTMLElement;

    expect(badge.textContent?.trim()).toBe('At Risk');
    expect(badge.classList).toContain('sla-risk');
  });
});
