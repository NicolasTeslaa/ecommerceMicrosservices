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

export interface PagedData<T> {
  items: T[];
  pagination?: PaginationMetadata;
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
  heightCm: number;
  widthCm: number;
  cubageM3: number;
  weightKg: number;
  originZipCode: string;
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
  customerId?: string;
  name: string;
  email: string;
}

export interface AuthApiResponse {
  userId: string;
  customerId: string;
  fullName: string;
  email: string;
  accessToken: string;
  expiresAtUtc: string;
}

export interface CheckoutData {
  zipCode: string;
  street: string;
  number: string;
  complement: string;
  neighborhood: string;
  city: string;
  state: string;
  country: string;
  reference: string;
  label: string;
  recipientName: string;
  paymentMethod: 'credit' | 'debit' | 'pix';
  cardNumber?: string;
  cardExpiry?: string;
  cardCvv?: string;
}

export interface CustomerAddress {
  id: string;
  customerId: string;
  label: string;
  recipientName: string;
  street: string;
  number: string;
  complement: string;
  neighborhood: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  reference: string;
  isDefault: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface ShippingQuote {
  provider: string;
  amount: number;
  estimatedDays: number;
  estimatedDeliveryDescription: string;
}

export interface CreateOrderPayload {
  customerId: string;
  customerAddressId: string;
  shippingAmount: number;
  paymentMethod: 'Credit' | 'Debit' | 'Pix';
  paymentToken?: string;
  paymentCardBrand?: string;
  paymentCardLast4?: string;
  items: Array<{
    productId: string;
    productName: string;
    unitPrice: number;
    quantity: number;
  }>;
}

export interface OrderApiResponse {
  orderId: string;
  status: string;
  message: string;
  requestedAtUtc: string;
}

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
}

export interface Order {
  id: string;
  customerId: string;
  customerAddressId: string;
  customerEmail: string;
  shippingAddress: string;
  shippingAmount: number;
  paymentMethod: string;
  paymentCardBrand?: string;
  paymentCardLast4?: string;
  totalAmount: number;
  status: string | number;
  rejectionReason?: string | number | null;
  rejectionDetail?: string | null;
  createdAtUtc: string;
  items: OrderItem[];
}

export interface ViaCepResponse {
  cep?: string;
  logradouro?: string;
  complemento?: string;
  bairro?: string;
  localidade?: string;
  uf?: string;
  erro?: boolean;
}

export interface OrderConfirmation {
  orderId: string;
  items: CartItem[];
  total: number;
  paymentMethod: string;
  status: 'approved' | 'pending';
  date: string;
  shippingAmount?: number;
  shippingAddress?: string;
}

export interface PaymentConfig {
  publishableKey: string;
}

export interface Payment {
  id: string;
  orderId: string;
  customerId: string;
  amount: number;
  currency: string;
  paymentMethod: 'Card' | 'Pix' | 'Unknown' | string;
  stripePaymentIntentId?: string | null;
  stripeClientSecret?: string | null;
  status: 'Pending' | 'PendingConfirmation' | 'RequiresAction' | 'Approved' | 'Failed' | 'Cancelled' | string;
  failureReason?: string | null;
  failureDetail?: string | null;
  attemptCount: number;
  maxAttemptsReached: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}
