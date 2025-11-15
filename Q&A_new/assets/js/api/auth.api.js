// assets/js/api/auth.api.js
import { http } from '../core/http.js';

// Spec: POST /api/v1/auth/login (LoginRequest {email, password})
// returns AuthResponse { accessToken, expiresIn, refreshToken }
export const authApi = {
  login(email, password) {
    return http.post('/auth/login', { email, password });
  },
};
