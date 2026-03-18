import { beforeEach, describe, expect, it } from 'vitest';
import { useAuth } from '@/store/useAuth';

describe('useAuth', () => {
  beforeEach(() => {
    useAuth.setState({ user: null, isAuthenticated: false });
  });

  it('creates a stable normalized user id on login', async () => {
    const success = await useAuth.getState().login('Alice@Example.com ', 'secret');

    expect(success).toBe(true);
    expect(useAuth.getState().user).toEqual({
      id: 'user-alice@example.com',
      name: 'alice',
      email: 'alice@example.com',
    });
  });

  it('creates a stable normalized user id on register', async () => {
    const success = await useAuth.getState().register('Alice', 'Alice@Example.com ', 'secret');

    expect(success).toBe(true);
    expect(useAuth.getState().user).toEqual({
      id: 'user-alice@example.com',
      name: 'Alice',
      email: 'alice@example.com',
    });
  });
});
