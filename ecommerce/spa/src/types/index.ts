export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  error?: ApiError;
  pagination?: PaginationMetadata;
}

export interface ApiError {
  code: string;
  message: string;
}

export interface PaginationMetadata {
  pageNumber: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  active: boolean;
  categoryId: string;
  categoryName?: string;
}

export interface Category {
  id: string;
  name: string;
  description?: string;
}

export interface CartItem {
  id: string;
  product: Product;
  quantity: number;
}

export type CartOwnerType = 'guest' | 'user';

export interface CartApiItem {
  id: string;
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  subtotal: number;
}

export interface CartApiResponse {
  id: string;
  ownerId: string;
  ownerType: CartOwnerType | 'Guest' | 'User';
  status: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  totalAmount: number;
  items: CartApiItem[];
}

export interface User {
  id: string;
  name: string;
  email: string;
}

export interface CheckoutData {
  fullName: string;
  email: string;
  address: string;
  city: string;
  zipCode: string;
  paymentMethod: 'credit' | 'debit' | 'pix';
  cardNumber?: string;
  cardExpiry?: string;
  cardCvv?: string;
}

export interface OrderConfirmation {
  orderId: string;
  items: CartItem[];
  total: number;
  paymentMethod: string;
  status: 'approved' | 'pending';
  date: string;
}
