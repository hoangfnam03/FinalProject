// assets/js/core/auth.js
import { storage } from './storage.js';

export const auth = {
  isAuthenticated() {
    const a = storage.getAuth();
    if (!a?.accessToken || !a?.expiresIn || !a?.savedAt) return false;
    const expiresAt = a.savedAt + a.expiresIn * 1000;
    return Date.now() < expiresAt;
  },
  getAccessToken() {
    return storage.getAuth()?.accessToken || null;
  },
  loginSuccess(authResponse) {
    // AuthResponse: accessToken, expiresIn, refreshToken
    storage.saveAuth({
      accessToken: authResponse.accessToken,
      refreshToken: authResponse.refreshToken,
      expiresIn: authResponse.expiresIn,
    });
  },
  logout() {
    storage.clearAuth();
  },
};
