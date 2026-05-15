import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { ApiErrorParserService } from '../../core/services/api-error-parser.service';
import { UserManagementService } from './user-management.service';
import { UserFormComponent } from './user-form.component';

describe('UserFormComponent', () => {
  let userManagement: jasmine.SpyObj<Pick<UserManagementService, 'getUser' | 'createUser' | 'updateUser'>>;
  let router: jasmine.SpyObj<Pick<Router, 'navigate'>>;

  function configure(id?: string): ComponentFixture<UserFormComponent> {
    userManagement = jasmine.createSpyObj<Pick<UserManagementService, 'getUser' | 'createUser' | 'updateUser'>>(
      'UserManagementService',
      ['getUser', 'createUser', 'updateUser']
    );
    router = jasmine.createSpyObj<Pick<Router, 'navigate'>>('Router', ['navigate']);
    userManagement.getUser.and.returnValue(of({
      id: id ?? 'user-1',
      email: 'marisol.vega@opssphere.test',
      firstName: 'Marisol',
      lastName: 'Vega',
      displayName: 'Marisol Vega',
      isActive: true,
      createdAt: '2026-05-14T00:00:00Z',
      roles: []
    }));

    TestBed.configureTestingModule({
      imports: [UserFormComponent],
      providers: [
        provideNoopAnimations(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap(id ? { id } : {})
            }
          }
        },
        { provide: Router, useValue: router },
        { provide: UserManagementService, useValue: userManagement },
        {
          provide: ApiErrorParserService,
          useValue: {
            parse: () => ({ message: 'Request failed.' })
          }
        }
      ]
    });

    const fixture = TestBed.createComponent(UserFormComponent);
    fixture.detectChanges();
    return fixture;
  }

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('requires create fields including temporary password', () => {
    const fixture = configure();
    const component = fixture.componentInstance as any;

    component.submit();

    expect(component.form.invalid).toBeTrue();
    expect(component.form.controls.email.hasError('required')).toBeTrue();
    expect(component.form.controls.firstName.hasError('required')).toBeTrue();
    expect(component.form.controls.lastName.hasError('required')).toBeTrue();
    expect(component.form.controls.temporaryPassword.hasError('required')).toBeTrue();
    expect(userManagement.createUser).not.toHaveBeenCalled();
  });

  it('creates users with trimmed values and default display name', () => {
    const fixture = configure();
    const component = fixture.componentInstance as any;
    userManagement.createUser.and.returnValue(of({
      id: 'user-1',
      email: 'marisol.vega@opssphere.test',
      firstName: 'Marisol',
      lastName: 'Vega',
      displayName: 'Marisol Vega',
      isActive: true,
      createdAt: '2026-05-14T00:00:00Z',
      roles: []
    }));

    component.form.setValue({
      email: 'marisol.vega@opssphere.test',
      firstName: ' Marisol ',
      lastName: ' Vega ',
      displayName: '',
      temporaryPassword: 'FictionalPass123!'
    });
    component.submit();

    expect(userManagement.createUser).toHaveBeenCalledOnceWith({
      email: 'marisol.vega@opssphere.test',
      firstName: 'Marisol',
      lastName: 'Vega',
      displayName: 'Marisol Vega',
      temporaryPassword: 'FictionalPass123!'
    });
    expect(router.navigate).toHaveBeenCalledOnceWith(['/admin/users', 'user-1']);
  });

  it('does not require temporary password when editing', () => {
    const fixture = configure('user-1');
    const component = fixture.componentInstance as any;
    userManagement.updateUser.and.returnValue(of({
      id: 'user-1',
      email: 'marisol.ortega@opssphere.test',
      firstName: 'Marisol',
      lastName: 'Ortega',
      displayName: 'Marisol Ortega',
      isActive: true,
      createdAt: '2026-05-14T00:00:00Z',
      roles: []
    }));

    component.form.patchValue({
      email: 'marisol.ortega@opssphere.test',
      firstName: 'Marisol',
      lastName: 'Ortega',
      displayName: 'Marisol Ortega',
      temporaryPassword: ''
    });
    component.submit();

    expect(component.form.controls.temporaryPassword.hasError('required')).toBeFalse();
    expect(userManagement.updateUser).toHaveBeenCalledOnceWith('user-1', {
      email: 'marisol.ortega@opssphere.test',
      firstName: 'Marisol',
      lastName: 'Ortega',
      displayName: 'Marisol Ortega'
    });
  });
});
