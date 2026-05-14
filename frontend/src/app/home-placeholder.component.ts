import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-home-placeholder',
  imports: [MatIconModule],
  template: `
    <section class="home-placeholder" aria-labelledby="home-title">
        <mat-icon aria-hidden="true">settings_applications</mat-icon>
        <div>
        <h1 id="home-title">Dashboard shell</h1>
        <p>Authenticated routing is ready for future operational workflows.</p>
      </div>
    </section>
  `,
  styles: [`
    .home-placeholder {
      display: flex;
      align-items: center;
      gap: 1rem;
      max-width: 720px;
      padding: 1.5rem 0;
    }

    mat-icon {
      flex: 0 0 auto;
      width: 2.5rem;
      height: 2.5rem;
      color: #2563eb;
      font-size: 2.5rem;
    }

    h1 {
      margin: 0 0 0.35rem;
      color: #111827;
      font-size: clamp(1.8rem, 3vw, 2.5rem);
      line-height: 1.1;
    }

    p {
      margin: 0;
      color: #4b5563;
      line-height: 1.6;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomePlaceholderComponent {}
