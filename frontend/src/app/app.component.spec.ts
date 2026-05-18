import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideRouter } from '@angular/router';

import { AuthService } from './core/auth/auth.service';
import { AppComponent } from './app.component';

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: {
            isAuthenticated: signal(false),
            currentUser: signal(null),
            hasRole: jasmine.createSpy('hasRole').and.returnValue(false),
            hasPermission: jasmine.createSpy('hasPermission').and.returnValue(false),
            hasAnyPermission: jasmine.createSpy('hasAnyPermission').and.returnValue(false),
            logout: jasmine.createSpy('logout')
          }
        }
      ]
    }).compileComponents();
  });

  it('creates the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;

    expect(app).toBeTruthy();
  });
});
