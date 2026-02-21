import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-card">

        <div class="brand">
          <span class="brand-dot"></span>TableVine
        </div>
        <h1 class="auth-title">Create your account</h1>
        <p class="auth-sub">Join thousands of diners discovering great restaurants</p>

        <form [formGroup]="form" (ngSubmit)="onSubmit()">

          <div class="form-row">
            <div class="form-group">
              <label>First name</label>
              <input type="text" formControlName="firstName" placeholder="Jane"
                     [class.invalid]="touched('firstName')">
            </div>
            <div class="form-group">
              <label>Last name</label>
              <input type="text" formControlName="lastName" placeholder="Smith"
                     [class.invalid]="touched('lastName')">
            </div>
          </div>

          <div class="form-group">
            <label>Email address</label>
            <input type="email" formControlName="email" autocomplete="email"
                   placeholder="you@example.com" [class.invalid]="touched('email')">
          </div>

          <div class="form-group">
            <label>Phone <span class="optional">(optional)</span></label>
            <input type="tel" formControlName="phone" placeholder="+1 555 000 0000">
          </div>

          <div class="form-group">
            <label>Password</label>
            <input type="password" formControlName="password" autocomplete="new-password"
                   placeholder="Min. 8 characters" [class.invalid]="touched('password')">
            @if (touched('password')) {
              <span class="field-error">Password must be at least 8 characters</span>
            }
          </div>

          <div class="form-group">
            <label>Account type</label>
            <select formControlName="role">
              <option value="Customer">Customer — discover &amp; book restaurants</option>
              <option value="Owner">Restaurant Owner — list my restaurant</option>
            </select>
          </div>

          @if (error) {
            <div class="error-banner">{{ error }}</div>
          }

          <button type="submit" class="btn-submit" [disabled]="loading">
            {{ loading ? 'Creating account…' : 'Create account' }}
          </button>
        </form>

        <p class="switch-link">
          Already have an account? <a routerLink="/auth/login">Sign in →</a>
        </p>

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
      width: 100%; max-width: 500px;
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

    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .form-group { margin-bottom: 18px; }
    label {
      display: block; font-size: 11px; font-weight: 500;
      letter-spacing: .12em; text-transform: uppercase;
      color: var(--warm-gray); margin-bottom: 7px;
    }
    .optional { text-transform: none; letter-spacing: 0; font-weight: 400; }
    input, select {
      width: 100%; padding: 12px 14px; box-sizing: border-box;
      border: 1px solid var(--border); border-radius: 6px;
      font-family: 'DM Sans', sans-serif; font-size: 14px; color: var(--ink);
      background: var(--cream); outline: none;
      appearance: none; transition: border-color .2s;
    }
    input:focus, select:focus { border-color: var(--wine); }
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
  `]
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  form = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: [''],
    password: ['', [Validators.required, Validators.minLength(8)]],
    role: ['Customer']
  });

  loading = false;
  error = '';

  touched(field: string): boolean {
    const c = this.form.get(field)!;
    return c.invalid && c.touched;
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';
    const v = this.form.value;
    this.auth.register({
      firstName: v.firstName!, lastName: v.lastName!, email: v.email!,
      password: v.password!, phone: v.phone || undefined, role: v.role || 'Customer'
    }).subscribe({
      next: () => this.router.navigate(['/']),
      error: err => { this.loading = false; this.error = err.error?.message ?? 'Registration failed.'; }
    });
  }
}
