// assets/js/core/storage.js
const KEY = 'intraqa.auth';

export const storage = {
  saveAuth(auth) {
    // auth: { accessToken, refreshToken, expiresIn, savedAt }
    const payload = { ...auth, savedAt: Date.now() };
    localStorage.setItem(KEY, JSON.stringify(payload));
    return payload;
  },
  getAuth() {
    try {
      const raw = localStorage.getItem(KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  },
  clearAuth() {
    localStorage.removeItem(KEY);
  },
};
