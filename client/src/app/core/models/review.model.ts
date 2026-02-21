export interface ReviewDto {
  id: string;
  restaurantId: string;
  customerId: string;
  customerName: string;
  reservationId: string;
  rating: number;
  comment?: string;
  isPublished: boolean;
  createdAt: string;
}

export interface CreateReviewDto {
  reservationId: string;
  rating: number;
  comment?: string;
}
