import { Component, computed, effect, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ClerkService } from 'ngx-clerk';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink, ButtonModule, CardModule],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss',
})
export class LandingComponent {
  protected readonly clerk = inject(ClerkService);
  protected readonly currentYear = new Date().getFullYear();
  private readonly router = inject(Router);

  constructor() {
    effect(() => {
      if (this.clerk.isLoaded() && this.clerk.isSignedIn()) {
        void this.router.navigate(['/dashboard']);
      }
    });
  }

  protected readonly userName = computed<string>(() => {
    const user = this.clerk.user();
    if (!user) return '';
    return user.fullName ?? user.firstName ?? user.username ?? '';
  });

  openSignIn(): void {
    this.clerk.openSignIn();
  }

  openSignUp(): void {
    this.clerk.openSignUp();
  }

  goToApp(): void {
    if (this.clerk.isSignedIn()) {
      this.router.navigate(['/dashboard']);
    } else {
      this.clerk.openSignUp();
    }
  }

  signOut(): void {
    this.clerk.signOut();
  }
}
