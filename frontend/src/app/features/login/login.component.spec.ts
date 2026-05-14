import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { FormGroup } from '@angular/forms';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  let fixture: ComponentFixture<LoginComponent>;
  let component: LoginComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('requires email and password before submit', () => {
    const form = (component as unknown as { form: FormGroup }).form;

    expect(form.invalid).toBeTrue();

    form.patchValue({ email: 'agent.novabank@opssphere.local' });

    expect(form.invalid).toBeTrue();

    form.patchValue({ password: 'OpsSphere123!' });

    expect(form.valid).toBeTrue();
  });
});
