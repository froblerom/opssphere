import { routes } from './app.routes';
import { AppPermissions } from './core/auth/auth-permissions';
import { permissionGuard } from './core/guards/auth.guard';

describe('app organization routes', () => {
  it('guards organization administration routes with permissionGuard metadata', () => {
    const organizationRoutes = routes.filter((route) => route.path?.startsWith('admin/organization'));

    expect(organizationRoutes.length).toBe(6);
    for (const route of organizationRoutes) {
      expect(route.canActivate).toContain(permissionGuard);
      expect(route.data?.['permissions']).toBeTruthy();
    }
  });

  it('sets organization entity kind route data for entity manager screens', () => {
    expect(routeData('admin/organization/regions')).toEqual({
      permissions: [AppPermissions.OrganizationView],
      kind: 'regions'
    });
    expect(routeData('admin/organization/countries')).toEqual({
      permissions: [AppPermissions.OrganizationView],
      kind: 'countries'
    });
    expect(routeData('admin/organization/accounts')).toEqual({
      permissions: [AppPermissions.OrganizationView],
      kind: 'accounts'
    });
    expect(routeData('admin/organization/campaigns')).toEqual({
      permissions: [AppPermissions.OrganizationView],
      kind: 'campaigns'
    });
  });

  it('requires assignment or scope management permission for assignments screen', () => {
    expect(routeData('admin/organization/assignments')?.['permissions']).toEqual([
      AppPermissions.AssignmentsManage,
      AppPermissions.ScopesManage
    ]);
  });
});

function routeData(path: string) {
  return routes.find((route) => route.path === path)?.data;
}
