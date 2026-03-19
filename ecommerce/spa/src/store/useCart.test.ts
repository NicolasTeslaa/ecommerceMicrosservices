import { beforeEach, describe, expect, it, vi } from 'vitest';
import { cartService } from '@/services/backendApi';
import { useAuth } from '@/store/useAuth';
import { useCart } from '@/store/useCart';

vi.mock('@/services/backendApi', () => ({
  cartService: {
    getCart: vi.fn(),
    addItem: vi.fn(),
    updateItemQuantity: vi.fn(),
    removeItem: vi.fn(),
    clearCart: vi.fn(),
  },
}));

describe('useCart', () => {
  beforeEach(() => {
    localStorage.clear();
    vi.clearAllMocks();

    useAuth.setState({
      user: null,
      isAuthenticated: false,
      accessToken: null,
      expiresAtUtc: null,
    });

    useCart.setState({
      guestId: 'guest-123',
      items: [
        {
          id: 'item-1',
          product: {
            id: 'product-1',
            name: 'Mouse Gamer',
            description: 'desc',
            price: 199.9,
            stockQuantity: 5,
            active: true,
            categoryId: 'cat-1',
            heightCm: 5,
            widthCm: 12,
            cubageM3: 0.01,
            weightKg: 0.3,
            originZipCode: '01001-000',
          },
          quantity: 2,
        },
      ],
      initialized: false,
    });
  });

  it('merges guest cart into authenticated cart during initialization', async () => {
    useAuth.setState({
      user: {
        id: 'user-1',
        customerId: 'customer-1',
        name: 'Alice',
        email: 'alice@example.com',
      },
      isAuthenticated: true,
      accessToken: 'token-123',
      expiresAtUtc: '2026-03-18T12:00:00Z',
    });

    vi.mocked(cartService.getCart)
      .mockResolvedValueOnce({
        id: 'guest-cart',
        ownerId: 'guest-123',
        ownerType: 'Guest',
        status: 'Active',
        createdAtUtc: '2026-03-18T10:00:00Z',
        updatedAtUtc: '2026-03-18T10:00:00Z',
        totalAmount: 399.8,
        items: [
          {
            id: 'item-1',
            productId: 'product-1',
            productName: 'Mouse Gamer',
            unitPrice: 199.9,
            quantity: 2,
            subtotal: 399.8,
          },
        ],
      })
      .mockResolvedValueOnce({
        id: 'user-cart',
        ownerId: 'user-1',
        ownerType: 'User',
        status: 'Active',
        createdAtUtc: '2026-03-18T10:01:00Z',
        updatedAtUtc: '2026-03-18T10:01:00Z',
        totalAmount: 0,
        items: [],
      });

    vi.mocked(cartService.addItem).mockResolvedValue({
      id: 'user-cart',
      ownerId: 'user-1',
      ownerType: 'User',
      status: 'Active',
      createdAtUtc: '2026-03-18T10:01:00Z',
      updatedAtUtc: '2026-03-18T10:02:00Z',
      totalAmount: 399.8,
      items: [
        {
          id: 'user-item-1',
          productId: 'product-1',
          productName: 'Mouse Gamer',
          unitPrice: 199.9,
          quantity: 2,
          subtotal: 399.8,
        },
      ],
    });

    vi.mocked(cartService.clearCart).mockResolvedValue({
      id: 'guest-cart',
      ownerId: 'guest-123',
      ownerType: 'Guest',
      status: 'Active',
      createdAtUtc: '2026-03-18T10:00:00Z',
      updatedAtUtc: '2026-03-18T10:03:00Z',
      totalAmount: 0,
      items: [],
    });

    await useCart.getState().initializeCart();

    expect(cartService.getCart).toHaveBeenNthCalledWith(1, 'guest', 'guest-123');
    expect(cartService.getCart).toHaveBeenNthCalledWith(2, 'user', 'user-1');
    expect(cartService.addItem).toHaveBeenCalledWith('user', 'user-1', {
      productId: 'product-1',
      productName: 'Mouse Gamer',
      unitPrice: 199.9,
      quantity: 2,
    });
    expect(cartService.clearCart).toHaveBeenCalledWith('guest', 'guest-123');
    expect(useCart.getState().items).toHaveLength(1);
    expect(useCart.getState().items[0].quantity).toBe(2);
  });
});
