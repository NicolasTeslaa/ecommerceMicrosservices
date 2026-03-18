import axios from 'axios';
import type {
  ApiResponse,
  CartApiResponse,
  CartOwnerType,
  Category,
  Product,
} from '@/types';

const backendApi = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5100',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

const unwrap = <T>(response: ApiResponse<T>): T => response.data;

const ownerPath = (ownerType: CartOwnerType, ownerId: string) =>
  `/api/cart/${ownerType}/${encodeURIComponent(ownerId)}`;

const applyCategoryNames = (products: Product[], categories: Category[]): Product[] => {
  const categoryById = new Map(categories.map((category) => [category.id, category.name]));

  return products.map((product) => ({
    ...product,
    categoryName: product.categoryName ?? categoryById.get(product.categoryId),
  }));
};

export const productService = {
  async getAll(): Promise<Product[]> {
    const [{ data: productResponse }, { data: categoryResponse }] = await Promise.all([
      backendApi.get<ApiResponse<Product[]>>('/api/catalog/products'),
      backendApi.get<ApiResponse<Category[]>>('/api/catalog/categories'),
    ]);

    return applyCategoryNames(unwrap(productResponse), unwrap(categoryResponse));
  },

  async getById(id: string): Promise<Product | undefined> {
    const [{ data: productResponse }, { data: categoryResponse }] = await Promise.all([
      backendApi.get<ApiResponse<Product>>(`/api/catalog/products/${id}`),
      backendApi.get<ApiResponse<Category[]>>('/api/catalog/categories'),
    ]);

    return applyCategoryNames([unwrap(productResponse)], unwrap(categoryResponse))[0];
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

export default backendApi;
