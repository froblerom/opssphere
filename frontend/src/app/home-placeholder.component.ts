import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home-placeholder',
  imports: [],
  template: '',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomePlaceholderComponent implements OnInit {
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.router.navigate(['/dashboard']);
  }
}
