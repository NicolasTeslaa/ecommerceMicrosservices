import { AxiosHeaders } from 'axios';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import backendApi, { authService, AUTH_TOKEN_STORAGE_KEY, productService } from '@/services/backendApi';

describe('backendApi productService', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('enriches products with category names from category lookup', async () => {
    vi.spyOn(backendApi, 'get')
      .mockResolvedValueOnce({
        data: {
          success: true,
          data: [
            {
              id: 'prod-1',
              name: 'GPU X',
              description: 'desc',
              price: 100,
              stockQuantity: 3,
              active: true,
              categoryId: 'cat-1',
            },
          ],
        },
      } as never)
      .mockResolvedValueOnce({
        data: {
          success: true,
          data: [
            {
              id: 'cat-1',
              name: 'Placas de Video',
            },
          ],
        },
      } as never);

    const products = await productService.getAll();

    expect(products[0].categoryName).toBe('Placas de Video');
  });

  it('submits register payload with fullName', async () => {
    const postSpy = vi.spyOn(backendApi, 'post').mockResolvedValue({
      data: {
        success: true,
        data: {
          userId: 'user-1',
          customerId: 'customer-1',
          fullName: 'Alice Example',
          email: 'alice@example.com',
          accessToken: 'token-123',
          expiresAtUtc: '2026-03-18T12:00:00Z',
        },
      },
    } as never);

    await authService.register({
      fullName: 'Alice Example',
      email: 'alice@example.com',
      password: 'secret123',
    });

    expect(postSpy).toHaveBeenCalledWith('/api/auth/register', {
      fullName: 'Alice Example',
      email: 'alice@example.com',
      password: 'secret123',
    });
  });

  it('attaches the bearer token when present in localStorage', async () => {
    localStorage.setItem(AUTH_TOKEN_STORAGE_KEY, 'token-123');

    const interceptor = backendApi.interceptors.request.handlers[0]?.fulfilled;
    const config = await interceptor?.({ headers: new AxiosHeaders() });

    expect(config?.headers.Authorization).toBe('Bearer token-123');
  });
});
