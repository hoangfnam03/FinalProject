// assets/js/api/auth.api.js
import { httpClient } from "../core/http.js";

export const authApi = {
  login: async (email, password) => {
    // Đường dẫn API login của BE (tự chỉnh)
    return httpClient.post("/api/auth/login", {
      email,
      password,
    });
  },
};
