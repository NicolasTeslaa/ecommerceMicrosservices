import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { cartService } from '@/services/backendApi';
import { useAuth } from '@/store/useAuth';
import type { CartApiResponse, CartItem, CartOwnerType, Product } from '@/types';

interface CartState {
  guestId: string;
  items: CartItem[];
  initialized: boolean;
  initializeCart: () => Promise<void>;
  addItem: (product: Product, quantity?: number) => Promise<void>;
  removeItem: (productId: string) => Promise<void>;
  updateQuantity: (productId: string, quantity: number) => Promise<void>;
  clearCart: () => Promise<void>;
  itemCount: () => number;
  subtotal: () => number;
}

const createGuestId = () => `guest-${crypto.randomUUID()}`;

const getCartOwner = (guestId: string) => {
  const { isAuthenticated, user } = useAuth.getState();

  if (isAuthenticated && user?.id) {
    return {
      ownerType: 'user' as CartOwnerType,
      ownerId: user.id,
      guestId,
    };
  }

  const nextGuestId = guestId || createGuestId();

  return {
    ownerType: 'guest' as CartOwnerType,
    ownerId: nextGuestId,
    guestId: nextGuestId,
  };
};

const mapProductSnapshot = (
  remoteItem: CartApiResponse['items'][number],
  currentItems: CartItem[],
  extraProducts: Product[] = []
): Product => {
  const knownProducts = new Map<string, Product>();

  for (const item of currentItems) {
    knownProducts.set(item.product.id, item.product);
  }

  for (const product of extraProducts) {
    knownProducts.set(product.id, product);
  }

  const previous = knownProducts.get(remoteItem.productId);

  return {
    id: remoteItem.productId,
    name: remoteItem.productName,
    description: previous?.description ?? '',
    price: remoteItem.unitPrice,
    stockQuantity: previous?.stockQuantity ?? 0,
    active: previous?.active ?? true,
    categoryId: previous?.categoryId ?? '',
    categoryName: previous?.categoryName,
  };
};

const mapCartItems = (
  cart: CartApiResponse,
  currentItems: CartItem[],
  extraProducts: Product[] = []
): CartItem[] =>
  cart.items.map((item) => ({
    id: item.id,
    product: mapProductSnapshot(item, currentItems, extraProducts),
    quantity: item.quantity,
  }));

export const useCart = create<CartState>()(
  persist(
    (set, get) => ({
      guestId: '',
      items: [],
      initialized: false,

      initializeCart: async () => {
        const owner = getCartOwner(get().guestId);
        const cart = await cartService.getCart(owner.ownerType, owner.ownerId);

        set({
          guestId: owner.guestId,
          items: mapCartItems(cart, get().items),
          initialized: true,
        });
      },

      addItem: async (product, quantity = 1) => {
        const owner = getCartOwner(get().guestId);
        const cart = await cartService.addItem(owner.ownerType, owner.ownerId, {
          productId: product.id,
          productName: product.name,
          unitPrice: product.price,
          quantity,
        });

        set({
          guestId: owner.guestId,
          items: mapCartItems(cart, get().items, [product]),
          initialized: true,
        });
      },

      removeItem: async (productId) => {
        const owner = getCartOwner(get().guestId);
        const cart = await cartService.removeItem(owner.ownerType, owner.ownerId, productId);

        set({
          guestId: owner.guestId,
          items: mapCartItems(cart, get().items),
          initialized: true,
        });
      },

      updateQuantity: async (productId, quantity) => {
        if (quantity < 0) {
          return;
        }

        if (quantity === 0) {
          await get().removeItem(productId);
          return;
        }

        const owner = getCartOwner(get().guestId);
        const cart = await cartService.updateItemQuantity(owner.ownerType, owner.ownerId, productId, quantity);

        set({
          guestId: owner.guestId,
          items: mapCartItems(cart, get().items),
          initialized: true,
        });
      },

      clearCart: async () => {
        const owner = getCartOwner(get().guestId);
        const cart = await cartService.clearCart(owner.ownerType, owner.ownerId);

        set({
          guestId: owner.guestId,
          items: mapCartItems(cart, get().items),
          initialized: true,
        });
      },

      itemCount: () => get().items.reduce((acc, item) => acc + item.quantity, 0),

      subtotal: () => get().items.reduce((acc, item) => acc + item.product.price * item.quantity, 0),
    }),
    {
      name: 'aura-cart',
      partialize: (state) => ({
        guestId: state.guestId,
      }),
    }
  )
);
