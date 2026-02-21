import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'auth',
    canActivate: [guestGuard],
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
      },
      { path: '', redirectTo: 'login', pathMatch: 'full' }
    ]
  },
  {
    path: 'restaurants',
    children: [
      {
        path: '',
        loadComponent: () => import('./features/restaurants/restaurant-list/restaurant-list.component').then(m => m.RestaurantListComponent)
      },
      {
        path: ':id',
        loadComponent: () => import('./features/restaurants/restaurant-detail/restaurant-detail.component').then(m => m.RestaurantDetailComponent)
      }
    ]
  },
  {
    path: 'reservations',
    canActivate: [authGuard],
    loadComponent: () => import('./features/reservations/my-reservations/my-reservations.component').then(m => m.MyReservationsComponent)
  },
  {
    path: 'owner',
    canActivate: [roleGuard(['Owner', 'Admin'])],
    children: [
      {
        path: '',
        loadComponent: () => import('./features/owner/owner-dashboard/owner-dashboard.component').then(m => m.OwnerDashboardComponent)
      },
      {
        path: 'restaurants/new',
        loadComponent: () => import('./features/owner/manage-restaurant/manage-restaurant.component').then(m => m.ManageRestaurantComponent)
      },
      {
        path: 'restaurants/:id',
        loadComponent: () => import('./features/owner/manage-restaurant/manage-restaurant.component').then(m => m.ManageRestaurantComponent)
      }
    ]
  },
  {
    path: 'admin',
    canActivate: [roleGuard(['Admin'])],
    children: [
      {
        path: '',
        redirectTo: 'restaurants',
        pathMatch: 'full'
      },
      {
        path: 'restaurants',
        loadComponent: () => import('./features/admin/admin-restaurants/admin-restaurants.component').then(m => m.AdminRestaurantsComponent)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
