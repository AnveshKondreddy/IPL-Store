import { Injectable, signal, computed } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly usernameSignal = signal<string | null>(this.stored());

  readonly username = this.usernameSignal.asReadonly();
  readonly isLoggedIn = computed(() => !!this.usernameSignal());

  login(username: string): void {
    const trimmed = username.trim();
    if (!trimmed) return;
    sessionStorage.setItem('ipl_user', trimmed);
    this.usernameSignal.set(trimmed);
  }

  logout(): void {
    sessionStorage.removeItem('ipl_user');
    this.usernameSignal.set(null);
  }

  private stored(): string | null {
    if (typeof sessionStorage === 'undefined') return null;
    return sessionStorage.getItem('ipl_user');
  }
}
