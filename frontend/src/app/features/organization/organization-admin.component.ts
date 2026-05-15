import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-organization-admin',
  imports: [RouterLink, MatButtonModule, MatIconModule],
  template: `
    <section class="org-home" aria-labelledby="org-title">
      <header>
        <p class="eyebrow">Administration</p>
        <h1 id="org-title">Organization</h1>
      </header>

      <div class="actions">
        <a mat-flat-button color="primary" routerLink="/admin/organization/regions"><mat-icon aria-hidden="true">public</mat-icon><span>Regions</span></a>
        <a mat-stroked-button routerLink="/admin/organization/countries"><mat-icon aria-hidden="true">flag</mat-icon><span>Countries</span></a>
        <a mat-stroked-button routerLink="/admin/organization/accounts"><mat-icon aria-hidden="true">business</mat-icon><span>Accounts</span></a>
        <a mat-stroked-button routerLink="/admin/organization/campaigns"><mat-icon aria-hidden="true">campaign</mat-icon><span>Campaigns</span></a>
        <a mat-stroked-button routerLink="/admin/organization/assignments"><mat-icon aria-hidden="true">assignment_ind</mat-icon><span>Assignments</span></a>
      </div>
    </section>
  `,
  styles: [`
    .org-home { display: grid; gap: 1.25rem; }
    h1 { margin: 0; color: #111827; font-size: 1.8rem; line-height: 1.2; }
    .eyebrow { margin: 0 0 0.2rem; color: #5b6475; font-size: 0.8rem; font-weight: 700; text-transform: uppercase; }
    .actions { display: flex; flex-wrap: wrap; gap: 0.75rem; }
    a { display: inline-flex; align-items: center; gap: 0.35rem; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OrganizationAdminComponent {}
