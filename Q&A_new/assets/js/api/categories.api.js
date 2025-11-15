// assets/js/api/categories.api.js
import { http } from '../core/http.js';

export const categoriesApi = {
  async list() {
    // GET /api/v1/categories
    const res = await http.get('/categories');
    return Array.isArray(res) ? res : (res?.items ?? []);
  },
};
