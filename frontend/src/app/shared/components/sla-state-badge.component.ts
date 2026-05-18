import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-sla-state-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span class="sla-badge" [ngClass]="badgeClass">
      {{ label }}
    </span>
  `,
  styles: [`
    .sla-badge {
      display: inline-flex;
      align-items: center;
      min-width: 84px;
      justify-content: center;
      border-radius: 6px;
      padding: 3px 8px;
      font-size: 0.78rem;
      font-weight: 700;
      line-height: 1.3;
      border: 1px solid transparent;
    }

    .sla-within { background: #e8f6ef; color: #17613a; border-color: #b7e4ca; }
    .sla-risk { background: #fff4d7; color: #7a5200; border-color: #f2d47c; }
    .sla-breached { background: #fde8e8; color: #9b1c1c; border-color: #f8b4b4; }
    .sla-completed { background: #eef2f7; color: #344054; border-color: #cbd5e1; }
  `]
})
export class SlaStateBadgeComponent {
  @Input() slaState: string | null | undefined;

  get label() {
    switch (this.slaState) {
      case 'WithinSla': return 'Within SLA';
      case 'AtRisk': return 'At Risk';
      case 'Breached': return 'Breached';
      case 'Completed': return 'Completed';
      default: return 'Not set';
    }
  }

  get badgeClass() {
    return {
      'sla-within': this.slaState === 'WithinSla',
      'sla-risk': this.slaState === 'AtRisk',
      'sla-breached': this.slaState === 'Breached',
      'sla-completed': this.slaState === 'Completed'
    };
  }
}
