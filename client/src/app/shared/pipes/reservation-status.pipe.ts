import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'reservationStatus', standalone: true })
export class ReservationStatusPipe implements PipeTransform {
  transform(status: string): string {
    const map: Record<string, string> = {
      'Pending': 'Pending',
      'Confirmed': 'Confirmed',
      'Completed': 'Completed',
      'CancelledByCustomer': 'Cancelled by You',
      'CancelledByRestaurant': 'Cancelled by Restaurant',
      'NoShow': 'No Show',
      '0': 'Pending',
      '1': 'Confirmed',
      '2': 'Completed',
      '3': 'Cancelled by You',
      '4': 'Cancelled by Restaurant',
      '5': 'No Show'
    };
    return map[status] ?? status;
  }
}
