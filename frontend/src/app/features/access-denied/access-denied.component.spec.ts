import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AccessDeniedComponent } from './access-denied.component';

describe('AccessDeniedComponent', () => {
  let fixture: ComponentFixture<AccessDeniedComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AccessDeniedComponent],
      providers: [provideRouter([])]
    });

    fixture = TestBed.createComponent(AccessDeniedComponent);
    fixture.detectChanges();
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('renders a safe access denied message', () => {
    expect(fixture.nativeElement.textContent).toContain('You do not have permission to access this page.');
  });

  it('includes a dashboard link', () => {
    const link = fixture.nativeElement.querySelector('a');

    expect(link?.getAttribute('href')).toBe('/dashboard');
  });
});
