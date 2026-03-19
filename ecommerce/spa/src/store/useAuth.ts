import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { AUTH_TOKEN_STORAGE_KEY, authService } from '@/services/backendApi';
import type { User } from '@/types';

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  accessToken: string | null;
  expiresAtUtc: string | null;
  login: (email: string, password: string) => Promise<boolean>;
  register: (name: string, email: string, password: string) => Promise<boolean>;
  logout: () => void;
}

const toUser = (payload: {
  userId: string;
  customerId: string;
  fullName: string;
  email: string;
}): User => ({
  id: payload.userId,
  customerId: payload.customerId,
  name: payload.fullName,
  email: payload.email,
});

const persistAccessToken = (token: string | null) => {
  if (typeof window === 'undefined') {
    return;
  }

  if (token) {
    window.localStorage.setItem(AUTH_TOKEN_STORAGE_KEY, token);
    return;
  }

  window.localStorage.removeItem(AUTH_TOKEN_STORAGE_KEY);
};

export const useAuth = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      isAuthenticated: false,
      accessToken: null,
      expiresAtUtc: null,

      login: async (email: string, password: string) => {
        const response = await authService.login({
          email: email.trim().toLowerCase(),
          password,
        });

        persistAccessToken(response.accessToken);
        set({
          user: toUser(response),
          isAuthenticated: true,
          accessToken: response.accessToken,
          expiresAtUtc: response.expiresAtUtc,
        });

        return true;
      },

      register: async (name: string, email: string, password: string) => {
        const response = await authService.register({
          fullName: name.trim(),
          email: email.trim().toLowerCase(),
          password,
        });

        persistAccessToken(response.accessToken);
        set({
          user: toUser(response),
          isAuthenticated: true,
          accessToken: response.accessToken,
          expiresAtUtc: response.expiresAtUtc,
        });

        return true;
      },

      logout: () => {
        persistAccessToken(null);
        set({ user: null, isAuthenticated: false, accessToken: null, expiresAtUtc: null });
      },
    }),
    { name: 'aura-auth' }
  )
);
