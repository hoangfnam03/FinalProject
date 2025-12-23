// assets/js/core/auth.js
import { saveToken, clearToken, getToken } from "./storage.js";

export function handleLoginSuccess(payload) {
  // tuỳ BE trả về gì, giả sử có field accessToken
  const token = payload?.accessToken || payload?.token;
  if (token) {
    saveToken(token);
  }
}

export function logout() {
  clearToken();
}

export function isAuthenticated() {
  return !!getToken();
}
