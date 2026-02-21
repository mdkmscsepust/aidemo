import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-card">

        <div class="brand">
          <span class="brand-dot"></span>TableVine
        </div>
        <h1 class="auth-title">Welcome back</h1>
        <p class="auth-sub">Sign in to manage your reservations</p>

        <form [formGroup]="form" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label>Email address</label>
            <input type="email" formControlName="email" autocomplete="email"
                   placeholder="you@example.com" [class.invalid]="emailInvalid">
            @if (emailInvalid) {
              <span class="field-error">Please enter a valid email</span>
            }
          </div>

          <div class="form-group">
            <label>Password</label>
            <input type="password" formControlName="password" autocomplete="current-password"
                   placeholder="••••••••" [class.invalid]="passInvalid">
            @if (passInvalid) {
              <span class="field-error">Password is required</span>
            }
          </div>

          @if (error) {
            <div class="error-banner">{{ error }}</div>
          }

          <button type="submit" class="btn-submit" [disabled]="loading">
            {{ loading ? 'Signing in…' : 'Sign in' }}
          </button>
        </form>

        <p class="switch-link">
          Don't have an account? <a routerLink="/auth/register">Create one →</a>
        </p>

        <div class="demo-hint">
          <span class="demo-label">Demo credentials</span>
          <span>alice&#64;demo.com · Customer123!</span>
        </div>

      </div>
    </div>
  `,
  styles: [`
    .auth-page {
      min-height: calc(100vh - 68px);
      display: flex; align-items: center; justify-content: center;
      background: var(--cream); padding: 32px 20px;
    }
    .auth-card {
      width: 100%; max-width: 440px;
      background: var(--card-bg); border: 1px solid var(--border);
      border-radius: var(--radius); padding: 48px 44px;
      box-shadow: var(--shadow);
    }

    .brand {
      font-family: 'Playfair Display', serif; font-size: 18px;
      color: var(--wine); display: flex; align-items: center; gap: 7px;
      margin-bottom: 28px;
    }
    .brand-dot {
      width: 7px; height: 7px; border-radius: 50%;
      background: var(--gold); display: inline-block;
    }
    .auth-title {
      font-family: 'Playfair Display', serif; font-size: 28px;
      color: var(--ink); margin: 0 0 6px;
    }
    .auth-sub { font-size: 14px; color: var(--warm-gray); margin: 0 0 32px; }

    .form-group { margin-bottom: 20px; }
    label {
      display: block; font-size: 11px; font-weight: 500;
      letter-spacing: .12em; text-transform: uppercase;
      color: var(--warm-gray); margin-bottom: 7px;
    }
    input {
      width: 100%; padding: 12px 14px; box-sizing: border-box;
      border: 1px solid var(--border); border-radius: 6px;
      font-family: 'DM Sans', sans-serif; font-size: 14px; color: var(--ink);
      background: var(--cream); outline: none; transition: border-color .2s;
    }
    input:focus { border-color: var(--wine); }
    input.invalid { border-color: #c0392b; }
    .field-error { display: block; font-size: 12px; color: #c0392b; margin-top: 5px; }

    .error-banner {
      background: #fdf2f2; border: 1px solid #f5c6cb; border-radius: 6px;
      padding: 11px 14px; font-size: 13px; color: #c0392b; margin-bottom: 20px;
    }

    .btn-submit {
      width: 100%; background: var(--wine); color: white; border: none;
      padding: 14px; border-radius: 6px; cursor: pointer;
      font-family: 'DM Sans', sans-serif; font-size: 14px; font-weight: 500;
      letter-spacing: .04em; transition: background .2s; margin-top: 4px;
    }
    .btn-submit:hover:not([disabled]) { background: var(--wine-light); }
    .btn-submit[disabled] { opacity: .6; cursor: not-allowed; }

    .switch-link {
      text-align: center; font-size: 13px; color: var(--warm-gray);
      margin: 24px 0 0;
    }
    .switch-link a { color: var(--wine); text-decoration: none; font-weight: 500; }
    .switch-link a:hover { text-decoration: underline; }

    .demo-hint {
      display: flex; flex-direction: column; align-items: center; gap: 4px;
      margin-top: 28px; padding-top: 20px; border-top: 1px solid var(--border);
      font-size: 12px; color: var(--warm-gray);
    }
    .demo-label { font-weight: 500; font-size: 11px; letter-spacing: .1em; text-transform: uppercase; }
  `]
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  loading = false;
  error = '';

  get emailInvalid(): boolean {
    const c = this.form.get('email')!;
    return c.invalid && c.touched;
  }
  get passInvalid(): boolean {
    const c = this.form.get('password')!;
    return c.invalid && c.touched;
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';
    const { email, password } = this.form.value;
    this.auth.login({ email: email!, password: password! }).subscribe({
      next: () => this.router.navigate(['/']),
      error: err => {
        this.loading = false;
        this.error = err.error?.message ?? 'Login failed. Please try again.';
      }
    });
  }
}
