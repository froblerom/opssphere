import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { ApiClientService } from '../../core/services/api-client.service';
import { OrganizationService } from './organization.service';

describe('OrganizationService', () => {
  let service: OrganizationService;
  let apiClient: jasmine.SpyObj<Pick<ApiClientService, 'get' | 'post' | 'put'>>;

  beforeEach(() => {
    apiClient = jasmine.createSpyObj<Pick<ApiClientService, 'get' | 'post' | 'put'>>('ApiClientService', ['get', 'post', 'put']);

    TestBed.configureTestingModule({
      providers: [
        OrganizationService,
        { provide: ApiClientService, useValue: apiClient }
      ]
    });

    service = TestBed.inject(OrganizationService);
  });

  it('calls list endpoints and unwraps response data', (done) => {
    const regions = [{ id: 'region-1', code: 'SKY', name: 'Sky Region', isActive: true, createdAt: '2026-05-14T00:00:00Z' }];
    apiClient.get.and.returnValue(of({ data: regions }));

    service.getRegions().subscribe((result) => {
      expect(result).toEqual(regions);
      expect(apiClient.get).toHaveBeenCalledOnceWith('regions');
      done();
    });
  });

  it('calls create and update endpoints for organization entities', (done) => {
    const request = { code: 'SKY', name: 'Sky Region' };
    const region = { id: 'region-1', ...request, isActive: true, createdAt: '2026-05-14T00:00:00Z' };
    apiClient.post.and.returnValue(of({ data: region }));
    apiClient.put.and.returnValue(of({ data: { ...region, name: 'Sky Operations' } }));

    service.createRegion(request).subscribe((created) => {
      expect(created).toEqual(region);
      expect(apiClient.post).toHaveBeenCalledOnceWith('regions', request);

      service.updateRegion('region-1', { code: 'SKY', name: 'Sky Operations' }).subscribe((updated) => {
        expect(updated.name).toBe('Sky Operations');
        expect(apiClient.put).toHaveBeenCalledWith('regions/region-1', { code: 'SKY', name: 'Sky Operations' });
        done();
      });
    });
  });

  it('calls deactivate endpoint for the selected entity kind', () => {
    apiClient.post.and.returnValue(of({}));

    service.deactivate('campaigns', 'campaign-1').subscribe();

    expect(apiClient.post).toHaveBeenCalledOnceWith('campaigns/campaign-1/deactivate', {});
  });

  it('calls user scope endpoints and unwraps assignments', (done) => {
    const assignment = {
      userId: 'user-1',
      email: 'viewer.lunara@opssphere.test',
      displayName: 'Viewer Lunara',
      isActive: true,
      roles: ['Viewer'],
      scopes: []
    };
    const request = { scopes: [{ scopeType: 'Country', countryId: 'country-1' }] };
    apiClient.get.and.returnValue(of({ data: assignment }));
    apiClient.put.and.returnValue(of({ data: { ...assignment, scopes: request.scopes } }));

    service.getUserScopes('user-1').subscribe((result) => {
      expect(result).toEqual(assignment);
      expect(apiClient.get).toHaveBeenCalledOnceWith('users/user-1/scopes');

      service.updateUserScopes('user-1', request).subscribe((updated) => {
        expect(updated.scopes).toEqual(request.scopes as any);
        expect(apiClient.put).toHaveBeenCalledOnceWith('users/user-1/scopes', request);
        done();
      });
    });
  });
});
