import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-manage-restaurant',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatSelectModule, MatSlideToggleModule,
    PageHeaderComponent, LoadingSpinnerComponent
  ],
  template: `
    <div class="page">
      <app-page-header
        [title]="isNew ? 'Add Restaurant' : 'Edit Restaurant'"
        [subtitle]="isNew ? 'Fill in the details to list your restaurant' : 'Update your restaurant information'" />

      @if (loading()) {
        <app-loading-spinner />
      } @else {
        <mat-card>
          <mat-card-content>
            <form [formGroup]="form" (ngSubmit)="onSubmit()">
              <h3>Basic Info</h3>
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Restaurant Name</mat-label>
                <input matInput formControlName="name">
                @if (form.get('name')?.hasError('required')) {
                  <mat-error>Name is required</mat-error>
                }
              </mat-form-field>

              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Description</mat-label>
                <textarea matInput formControlName="description" rows="3"></textarea>
              </mat-form-field>

              <div class="row">
                <mat-form-field appearance="outline" class="half">
                  <mat-label>Cuisine Type</mat-label>
                  <input matInput formControlName="cuisineType">
                </mat-form-field>
                <mat-form-field appearance="outline" class="half">
                  <mat-label>Price Tier</mat-label>
                  <mat-select formControlName="priceTier">
                    <mat-option value="Budget">$ Budget</mat-option>
                    <mat-option value="Moderate">$$ Moderate</mat-option>
                    <mat-option value="Upscale">$$$ Upscale</mat-option>
                    <mat-option value="Fine">$$$$ Fine Dining</mat-option>
                  </mat-select>
                </mat-form-field>
              </div>

              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Default Reservation Duration (minutes)</mat-label>
                <input matInput type="number" formControlName="defaultDurationMinutes">
              </mat-form-field>

              <h3>Location</h3>
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Address Line 1</mat-label>
                <input matInput formControlName="addressLine1">
              </mat-form-field>
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Address Line 2 (optional)</mat-label>
                <input matInput formControlName="addressLine2">
              </mat-form-field>

              <div class="row">
                <mat-form-field appearance="outline" class="half">
                  <mat-label>City</mat-label>
                  <input matInput formControlName="city">
                </mat-form-field>
                <mat-form-field appearance="outline" class="half">
                  <mat-label>State</mat-label>
                  <input matInput formControlName="state">
                </mat-form-field>
              </div>

              <div class="row">
                <mat-form-field appearance="outline" class="half">
                  <mat-label>Postal Code</mat-label>
                  <input matInput formControlName="postalCode">
                </mat-form-field>
                <mat-form-field appearance="outline" class="half">
                  <mat-label>Country</mat-label>
                  <input matInput formControlName="country">
                </mat-form-field>
              </div>

              <h3>Contact</h3>
              <div class="row">
                <mat-form-field appearance="outline" class="half">
                  <mat-label>Phone</mat-label>
                  <input matInput formControlName="phone">
                </mat-form-field>
                <mat-form-field appearance="outline" class="half">
                  <mat-label>Email</mat-label>
                  <input matInput type="email" formControlName="email">
                </mat-form-field>
              </div>
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Website</mat-label>
                <input matInput formControlName="website">
              </mat-form-field>

              @if (error()) { <p class="error-msg">{{ error() }}</p> }

              <div class="actions">
                <button mat-button type="button" (click)="router.navigate(['/owner'])">Cancel</button>
                <button mat-flat-button color="primary" type="submit" [disabled]="saving()">
                  {{ saving() ? 'Saving…' : (isNew ? 'Create Restaurant' : 'Save Changes') }}
                </button>
              </div>
            </form>
          </mat-card-content>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .page { max-width: 720px; margin: 0 auto; padding: 32px 24px; }
    .full-width { width: 100%; margin-bottom: 12px; }
    .row { display: flex; gap: 16px; }
    .half { flex: 1; margin-bottom: 12px; }
    h3 { font-size: 16px; font-weight: 600; margin: 20px 0 12px; color: #374151; border-bottom: 1px solid #f3f4f6; padding-bottom: 8px; }
    .actions { display: flex; justify-content: flex-end; gap: 12px; margin-top: 24px; }
    .error-msg { color: #ef4444; font-size: 14px; }
  `]
})
export class ManageRestaurantComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  readonly router = inject(Router);
  private readonly restaurantService = inject(RestaurantService);
  private readonly fb = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);

  isNew = true;
  restaurantId: string | null = null;
  loading = signal(false);
  saving = signal(false);
  error = signal('');

  form = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    cuisineType: [''],
    priceTier: ['Moderate'],
    defaultDurationMinutes: [90],
    addressLine1: ['', Validators.required],
    addressLine2: [''],
    city: ['', Validators.required],
    state: [''],
    postalCode: ['', Validators.required],
    country: ['US'],
    phone: [''],
    email: [''],
    website: ['']
  });

  ngOnInit(): void {
    this.restaurantId = this.route.snapshot.paramMap.get('id');
    this.isNew = !this.restaurantId || this.restaurantId === 'new';

    if (!this.isNew && this.restaurantId) {
      this.loading.set(true);
      this.restaurantService.getById(this.restaurantId).subscribe({
        next: res => {
          if (res.data) this.form.patchValue(res.data as any);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      });
    }
  }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    this.error.set('');
    const dto = this.form.value as any;
    const call = this.isNew
      ? this.restaurantService.create(dto)
      : this.restaurantService.update(this.restaurantId!, dto);

    call.subscribe({
      next: () => {
        this.snackBar.open(this.isNew ? 'Restaurant created! Awaiting approval.' : 'Changes saved.', 'OK', { duration: 4000 });
        this.router.navigate(['/owner']);
      },
      error: err => {
        this.saving.set(false);
        this.error.set(err.error?.message ?? 'Failed to save restaurant.');
      }
    });
  }
}
