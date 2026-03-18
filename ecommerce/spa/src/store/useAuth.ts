import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { User } from '@/types';

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<boolean>;
  register: (name: string, email: string, password: string) => Promise<boolean>;
  logout: () => void;
}

export const useAuth = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      isAuthenticated: false,

      login: async (email: string, _password: string) => {
        // Mock login
        await new Promise(r => setTimeout(r, 1200));
        const normalizedEmail = email.trim().toLowerCase();
        const user: User = {
          id: `user-${normalizedEmail}`,
          name: normalizedEmail.split('@')[0],
          email: normalizedEmail,
        };
        set({ user, isAuthenticated: true });
        return true;
      },

      register: async (name: string, email: string, _password: string) => {
        await new Promise(r => setTimeout(r, 1500));
        const normalizedEmail = email.trim().toLowerCase();
        const user: User = {
          id: `user-${normalizedEmail}`,
          name,
          email: normalizedEmail,
        };
        set({ user, isAuthenticated: true });
        return true;
      },

      logout: () => set({ user: null, isAuthenticated: false }),
    }),
    { name: 'aura-auth' }
  )
);
