import { beforeEach, describe, expect, it, vi } from 'vitest';
import backendApi, { productService } from '@/services/backendApi';

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
});
