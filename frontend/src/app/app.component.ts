import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';

import { AuthService } from './core/auth/auth.service';
import { AppPermissions, AppRoles } from './core/auth/auth-permissions';

// SECURITY NOTE: Role-aware navigation below is UX only.
// Backend authorization is the source of truth and is enforced independently of this component.

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterOutlet, MatButtonModule, MatIconModule, MatToolbarModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {
  protected readonly authService = inject(AuthService);
  protected readonly title = 'OpsSphere';
  private readonly router = inject(Router);

  // Role-aware navigation visibility — UX only, NOT security controls.
  protected readonly showAdminNav = computed(() =>
    this.authService.hasRole(AppRoles.Admin)
  );

  protected readonly showDashboardNav = computed(() =>
    this.authService.hasPermission(AppPermissions.DashboardView)
  );

  protected readonly showTicketsNav = computed(() =>
    this.authService.hasPermission(AppPermissions.TicketsView)
  );

  protected readonly showReportsNav = computed(() =>
    this.authService.hasPermission(AppPermissions.ReportsView)
  );

  protected readonly showAuditNav = computed(() =>
    this.authService.hasPermission(AppPermissions.AuditView)
  );

  protected readonly showUsersNav = computed(() =>
    this.authService.hasAnyPermission([AppPermissions.UsersView, AppPermissions.UsersManage])
  );

  protected readonly showOrganizationNav = computed(() =>
    this.authService.hasAnyPermission([AppPermissions.OrganizationView, AppPermissions.OrganizationManage])
  );

  protected logout(): void {
    this.authService.logout();
    void this.router.navigate(['/login']);
  }
}
