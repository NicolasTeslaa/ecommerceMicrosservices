import { create } from 'zustand';
import type { OrderConfirmation } from '@/types';

interface OrderState {
  lastOrder: OrderConfirmation | null;
  setLastOrder: (order: OrderConfirmation) => void;
  clearOrder: () => void;
}

export const useOrder = create<OrderState>()((set) => ({
  lastOrder: null,
  setLastOrder: (order) => set({ lastOrder: order }),
  clearOrder: () => set({ lastOrder: null }),
}));
