import axios from 'axios';
import type {
  ApiResponse,
  AuthApiResponse,
  CartApiResponse,
  CartOwnerType,
  Category,
  CreateOrderPayload,
  CustomerAddress,
  InventoryAvailability,
  Invoice,
  Payment,
  PaymentConfig,
  Order,
  PagedData,
  Product,
  ShippingQuote,
  ViaCepResponse,
  OrderApiResponse,
} from '@/types';

export const AUTH_TOKEN_STORAGE_KEY = 'aura-access-token';
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5100';

const backendApi = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

const unwrap = <T>(response: ApiResponse<T>): T => response.data;

backendApi.interceptors.request.use((config) => {
  const token = typeof window !== 'undefined'
    ? window.localStorage.getItem(AUTH_TOKEN_STORAGE_KEY)
    : null;

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

const ownerPath = (ownerType: CartOwnerType, ownerId: string) =>
  `/api/cart/${ownerType}/${encodeURIComponent(ownerId)}`;

const applyCategoryNames = (products: Product[], categories: Category[]): Product[] => {
  const categoryById = new Map(categories.map((category) => [category.id, category.name]));

  return products.map((product) => ({
    ...product,
    categoryName: product.categoryName ?? categoryById.get(product.categoryId),
  }));
};

const applyInventoryAvailability = (
  products: Product[],
  availability: InventoryAvailability[]
): Product[] => {
  const availabilityByProductId = new Map(availability.map((item) => [item.productId, item]));

  return products.map((product) => {
    const inventory = availabilityByProductId.get(product.id);

    if (!inventory) {
      return {
        ...product,
        stockQuantity: 0,
        reservedQuantity: 0,
        active: product.active && false,
      };
    }

    return {
      ...product,
      stockQuantity: inventory.availableQuantity,
      reservedQuantity: inventory.reservedQuantity,
      active: product.active && inventory.active,
    };
  });
};

export const productService = {
  async getPage(
    pageNumber = 1,
    pageSize = 10,
    filters?: {
      searchTerm?: string;
      categoryId?: string | null;
    }
  ): Promise<PagedData<Product>> {
    const [{ data: productResponse }, { data: categoryResponse }] = await Promise.all([
      backendApi.get<ApiResponse<Product[]>>('/api/catalog/products', {
        params: {
          pageNumber,
          pageSize,
          searchTerm: filters?.searchTerm || undefined,
          categoryId: filters?.categoryId || undefined,
        },
      }),
      backendApi.get<ApiResponse<Category[]>>('/api/catalog/categories'),
    ]);

    const catalogProducts = applyCategoryNames(unwrap(productResponse), unwrap(categoryResponse));
    const inventoryItems = await inventoryService.getAvailability(
      catalogProducts.map((product) => product.id)
    );

    return {
      items: applyInventoryAvailability(catalogProducts, inventoryItems),
      pagination: productResponse.pagination,
    };
  },

  async getAll(): Promise<Product[]> {
    const { items } = await productService.getPage(1, 100);
    return items;
  },

  async getById(id: string): Promise<Product | undefined> {
    const [{ data: productResponse }, { data: categoryResponse }, inventoryItem] = await Promise.all([
      backendApi.get<ApiResponse<Product>>(`/api/catalog/products/${id}`),
      backendApi.get<ApiResponse<Category[]>>('/api/catalog/categories'),
      inventoryService.getByProductId(id),
    ]);

    return applyInventoryAvailability(
      applyCategoryNames([unwrap(productResponse)], unwrap(categoryResponse)),
      inventoryItem ? [inventoryItem] : []
    )[0];
  },
};

export const inventoryService = {
  async getByProductId(productId: string): Promise<InventoryAvailability | null> {
    const { data } = await backendApi.get<ApiResponse<InventoryAvailability | null>>(`/api/inventory/products/${productId}`);
    return unwrap(data);
  },

  async getAvailability(productIds: string[]): Promise<InventoryAvailability[]> {
    if (productIds.length === 0) {
      return [];
    }

    const { data } = await backendApi.post<ApiResponse<InventoryAvailability[]>>('/api/inventory/products/availability', {
      productIds,
    });

    return unwrap(data);
  },
};

export const categoryService = {
  async getAll(): Promise<Category[]> {
    const { data } = await backendApi.get<ApiResponse<Category[]>>('/api/catalog/categories');
    return unwrap(data);
  },

  async getById(id: string): Promise<Category | undefined> {
    const { data } = await backendApi.get<ApiResponse<Category>>(`/api/catalog/categories/${id}`);
    return unwrap(data);
  },
};

export const cartService = {
  async getCart(ownerType: CartOwnerType, ownerId: string): Promise<CartApiResponse> {
    const { data } = await backendApi.get<ApiResponse<CartApiResponse>>(ownerPath(ownerType, ownerId));
    return unwrap(data);
  },

  async addItem(
    ownerType: CartOwnerType,
    ownerId: string,
    payload: {
      productId: string;
      productName: string;
      unitPrice: number;
      quantity: number;
    }
  ): Promise<CartApiResponse> {
    const { data } = await backendApi.post<ApiResponse<CartApiResponse>>(
      `${ownerPath(ownerType, ownerId)}/items`,
      payload
    );

    return unwrap(data);
  },

  async updateItemQuantity(
    ownerType: CartOwnerType,
    ownerId: string,
    productId: string,
    quantity: number
  ): Promise<CartApiResponse> {
    const { data } = await backendApi.put<ApiResponse<CartApiResponse>>(
      `${ownerPath(ownerType, ownerId)}/items/${productId}`,
      { quantity }
    );

    return unwrap(data);
  },

  async removeItem(ownerType: CartOwnerType, ownerId: string, productId: string): Promise<CartApiResponse> {
    const { data } = await backendApi.delete<ApiResponse<CartApiResponse>>(
      `${ownerPath(ownerType, ownerId)}/items/${productId}`
    );

    return unwrap(data);
  },

  async clearCart(ownerType: CartOwnerType, ownerId: string): Promise<CartApiResponse> {
    const { data } = await backendApi.delete<ApiResponse<CartApiResponse>>(ownerPath(ownerType, ownerId));
    return unwrap(data);
  },
};

export const authService = {
  async login(payload: { email: string; password: string }): Promise<AuthApiResponse> {
    const { data } = await backendApi.post<ApiResponse<AuthApiResponse>>('/api/auth/login', payload);
    return unwrap(data);
  },

  async register(payload: { fullName: string; email: string; password: string }): Promise<AuthApiResponse> {
    const { data } = await backendApi.post<ApiResponse<AuthApiResponse>>('/api/auth/register', payload);
    return unwrap(data);
  },

  async me(): Promise<{
    userId: string;
    customerId: string;
    email: string;
    fullName: string;
  }> {
    const { data } = await backendApi.get<
      ApiResponse<{
        userId: string;
        customerId: string;
        email: string;
        fullName: string;
      }>
    >('/api/auth/me');

    return unwrap(data);
  },
};

export const customerService = {
  async getAddresses(customerId: string): Promise<CustomerAddress[]> {
    const { data } = await backendApi.get<ApiResponse<CustomerAddress[]>>(`/api/customers/${customerId}/addresses`);
    return unwrap(data);
  },

  async createAddress(
    customerId: string,
    payload: Omit<CustomerAddress, 'id' | 'customerId' | 'createdAtUtc' | 'updatedAtUtc'>
  ): Promise<CustomerAddress> {
    const { data } = await backendApi.post<ApiResponse<CustomerAddress>>(`/api/customers/${customerId}/addresses`, payload);
    return unwrap(data);
  },
};

export const shippingService = {
  async calculateQuote(payload: {
    heightCm: number;
    widthCm: number;
    cubageM3: number;
    weightKg: number;
    originZipCode: string;
    destinationZipCode: string;
    provider?: string;
  }): Promise<ShippingQuote> {
    const { data } = await backendApi.post<ApiResponse<ShippingQuote>>('/api/shipping/quotes', payload);
    return unwrap(data);
  },
};

export const orderService = {
  async create(payload: CreateOrderPayload): Promise<OrderApiResponse> {
    const { data } = await backendApi.post<ApiResponse<OrderApiResponse>>('/api/orders', payload);
    return unwrap(data);
  },

  async cancel(orderId: string, customerId: string): Promise<{ orderId: string; status: string; message: string }> {
    const { data } = await backendApi.post<ApiResponse<{ orderId: string; status: string; message: string }>>(
      `/api/orders/${orderId}/cancel`,
      { customerId }
    );

    return unwrap(data);
  },

  async getByCustomer(customerId: string, pageNumber = 1, pageSize = 10): Promise<PagedData<Order>> {
    const { data } = await backendApi.get<ApiResponse<Order[]>>(`/api/orders/customers/${customerId}`, {
      params: {
        pageNumber,
        pageSize,
      },
    });

    return {
      items: unwrap(data),
      pagination: data.pagination,
    };
  },
};

export const paymentService = {
  async getConfig(): Promise<PaymentConfig> {
    const { data } = await backendApi.get<ApiResponse<PaymentConfig>>('/api/payments/config');
    return unwrap(data);
  },

  async getByOrderId(orderId: string): Promise<Payment | null> {
    const { data } = await backendApi.get<ApiResponse<Payment | null>>(`/api/payments/orders/${orderId}`);
    return unwrap(data);
  },
};

export const invoiceService = {
  async getByOrderId(orderId: string): Promise<Invoice> {
    const { data } = await backendApi.get<ApiResponse<Invoice>>(`/api/invoices/orders/${orderId}`);
    return unwrap(data);
  },
};

export const viaCepService = {
  async lookup(zipCode: string): Promise<ViaCepResponse> {
    const normalized = zipCode.replace(/\D/g, '');
    const response = await fetch(`https://viacep.com.br/ws/${normalized}/json/`);

    if (!response.ok) {
      throw new Error('Nao foi possivel consultar o CEP.');
    }

    return response.json() as Promise<ViaCepResponse>;
  },
};

export default backendApi;
