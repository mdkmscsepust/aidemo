export interface ReservationDto {
  id: string;
  restaurantId: string;
  restaurantName: string;
  tableId: string;
  tableNumber: string;
  tableCapacity: number;
  customerId: string;
  customerName: string;
  partySize: number;
  reservationDate: string;
  startTime: string;
  endTime: string;
  durationMinutes: number;
  status: string;
  specialRequests?: string;
  confirmationCode: string;
  notes?: string;
  cancelledAt?: string;
  cancellationReason?: string;
  createdAt: string;
}

export interface AvailableSlotDto {
  slotTime: string;
  tableId: string;
  tableNumber: string;
  tableCapacity: number;
}

export interface CreateReservationDto {
  restaurantId: string;
  tableId: string;
  reservationDate: string;
  slotTime: string;
  partySize: number;
  specialRequests?: string;
}
