export interface RestaurantListDto {
  id: string;
  name: string;
  cuisineType?: string;
  city: string;
  addressLine1: string;
  avgRating: number;
  reviewCount: number;
  priceTier: string;
  imageUrl?: string;
  isApproved: boolean;
}

export interface RestaurantDetailDto {
  id: string;
  ownerId: string;
  name: string;
  description?: string;
  cuisineType?: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state?: string;
  postalCode: string;
  country: string;
  phone?: string;
  email?: string;
  website?: string;
  avgRating: number;
  reviewCount: number;
  priceTier: string;
  defaultDurationMinutes: number;
  isApproved: boolean;
  isActive: boolean;
  imageUrl?: string;
  openingHours: OpeningHoursDto[];
  tables: TableDto[];
}

export interface OpeningHoursDto {
  id: string;
  dayOfWeek: string;
  openTime: string;
  closeTime: string;
  isClosed: boolean;
}

export interface TableDto {
  id: string;
  tableNumber: string;
  capacity: number;
  minCapacity: number;
  isActive: boolean;
  notes?: string;
}

export interface RestaurantSearchParams {
  search?: string;
  city?: string;
  cuisine?: string;
  priceMin?: number;
  priceMax?: number;
  page?: number;
  pageSize?: number;
  sortBy?: string;
}
