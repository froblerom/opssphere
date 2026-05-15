import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { ApiClientService } from '../../core/services/api-client.service';
import { UserManagementService } from './user-management.service';

describe('UserManagementService', () => {
  let service: UserManagementService;
  let apiClient: jasmine.SpyObj<Pick<ApiClientService, 'get' | 'post' | 'put'>>;

  beforeEach(() => {
    apiClient = jasmine.createSpyObj<Pick<ApiClientService, 'get' | 'post' | 'put'>>('ApiClientService', ['get', 'post', 'put']);

    TestBed.configureTestingModule({
      providers: [
        UserManagementService,
        { provide: ApiClientService, useValue: apiClient }
      ]
    });

    service = TestBed.inject(UserManagementService);
  });

  it('calls the user list endpoint and unwraps the response data', (done) => {
    const users = [
      {
        id: 'user-1',
        email: 'marisol.vega@opssphere.test',
        firstName: 'Marisol',
        lastName: 'Vega',
        displayName: 'Marisol Vega',
        isActive: true,
        createdAt: '2026-05-14T00:00:00Z',
        roles: []
      }
    ];
    apiClient.get.and.returnValue(of({ data: users }));

    service.getUsers().subscribe((result) => {
      expect(result).toEqual(users);
      expect(apiClient.get).toHaveBeenCalledOnceWith('users');
      done();
    });
  });

  it('calls create and update endpoints without adding sensitive response fields', (done) => {
    const createdUser = {
      id: 'user-1',
      email: 'marisol.vega@opssphere.test',
      firstName: 'Marisol',
      lastName: 'Vega',
      displayName: 'Marisol Vega',
      isActive: true,
      createdAt: '2026-05-14T00:00:00Z',
      roles: []
    };
    const createRequest = {
      email: 'marisol.vega@opssphere.test',
      firstName: 'Marisol',
      lastName: 'Vega',
      displayName: 'Marisol Vega',
      temporaryPassword: 'FictionalPass123!'
    };
    apiClient.post.and.returnValue(of({ data: createdUser }));
    apiClient.put.and.returnValue(of({ data: { ...createdUser, displayName: 'Marisol Ortega' } }));

    service.createUser(createRequest).subscribe((result) => {
      expect(result).toEqual(createdUser);
      expect(JSON.stringify(result)).not.toContain('temporaryPassword');
      expect(JSON.stringify(result)).not.toContain('passwordHash');
      expect(apiClient.post).toHaveBeenCalledOnceWith('users', createRequest);

      service.updateUser('user-1', {
        email: 'marisol.ortega@opssphere.test',
        firstName: 'Marisol',
        lastName: 'Ortega',
        displayName: 'Marisol Ortega'
      }).subscribe((updated) => {
        expect(updated.displayName).toBe('Marisol Ortega');
        expect(apiClient.put).toHaveBeenCalledWith('users/user-1', jasmine.any(Object));
        done();
      });
    });
  });

  it('calls role assignment endpoint with roleIds payload', (done) => {
    const roleIds = ['role-agent', 'role-viewer'];
    apiClient.put.and.returnValue(of({
      data: {
        id: 'user-1',
        email: 'marisol.vega@opssphere.test',
        firstName: 'Marisol',
        lastName: 'Vega',
        displayName: 'Marisol Vega',
        isActive: true,
        createdAt: '2026-05-14T00:00:00Z',
        roles: []
      }
    }));

    service.updateUserRoles('user-1', roleIds).subscribe(() => {
      expect(apiClient.put).toHaveBeenCalledOnceWith('users/user-1/roles', { roleIds });
      done();
    });
  });

  it('calls deactivate endpoint', () => {
    apiClient.post.and.returnValue(of({}));

    service.deactivateUser('user-1').subscribe();

    expect(apiClient.post).toHaveBeenCalledOnceWith('users/user-1/deactivate', {});
  });
});
