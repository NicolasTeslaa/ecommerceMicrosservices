import { beforeEach, describe, expect, it, vi } from 'vitest';
import { authService, AUTH_TOKEN_STORAGE_KEY } from '@/services/backendApi';
import { useAuth } from '@/store/useAuth';

describe('useAuth', () => {
  beforeEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
    useAuth.setState({ user: null, isAuthenticated: false, accessToken: null, expiresAtUtc: null });
  });

  it('stores the authenticated user and access token on login', async () => {
    vi.spyOn(authService, 'login').mockResolvedValue({
      userId: 'user-1',
      customerId: 'customer-1',
      fullName: 'Alice Example',
      email: 'alice@example.com',
      accessToken: 'token-123',
      expiresAtUtc: '2026-03-18T12:00:00Z',
    });

    const success = await useAuth.getState().login('Alice@Example.com ', 'secret');

    expect(success).toBe(true);
    expect(useAuth.getState().user).toEqual({
      id: 'user-1',
      customerId: 'customer-1',
      name: 'Alice Example',
      email: 'alice@example.com',
    });
    expect(useAuth.getState().accessToken).toBe('token-123');
    expect(localStorage.getItem(AUTH_TOKEN_STORAGE_KEY)).toBe('token-123');
  });

  it('maps name to fullName and stores auth payload on register', async () => {
    vi.spyOn(authService, 'register').mockResolvedValue({
      userId: 'user-2',
      customerId: 'customer-2',
      fullName: 'Alice',
      email: 'alice@example.com',
      accessToken: 'token-456',
      expiresAtUtc: '2026-03-18T13:00:00Z',
    });

    const success = await useAuth.getState().register('Alice', 'Alice@Example.com ', 'secret');

    expect(success).toBe(true);
    expect(useAuth.getState().user).toEqual({
      id: 'user-2',
      customerId: 'customer-2',
      name: 'Alice',
      email: 'alice@example.com',
    });
    expect(useAuth.getState().accessToken).toBe('token-456');
  });

  it('clears the token on logout', () => {
    localStorage.setItem(AUTH_TOKEN_STORAGE_KEY, 'token-123');
    useAuth.setState({
      user: { id: 'user-1', customerId: 'customer-1', name: 'Alice', email: 'alice@example.com' },
      isAuthenticated: true,
      accessToken: 'token-123',
      expiresAtUtc: '2026-03-18T12:00:00Z',
    });

    useAuth.getState().logout();

    expect(useAuth.getState().isAuthenticated).toBe(false);
    expect(useAuth.getState().user).toBeNull();
    expect(localStorage.getItem(AUTH_TOKEN_STORAGE_KEY)).toBeNull();
  });
});
