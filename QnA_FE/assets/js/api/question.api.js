// assets/js/api/question.api.js
import { http } from '../core/http.js';

export const questionApi = {
  /**
   * Dùng chung cho list câu hỏi với phân trang
   * GET /api/v1/posts?page=1&pageSize=10&sort=newest
   */
  getList({ page = 1, pageSize = 10, sort = 'newest', search, tag } = {}) {
    const params = {
      page,
      pageSize,
      sort,
    };

    if (search) params.search = search;
    if (tag) params.tag = tag;

    return http.get('/api/v1/posts', { params });
  },

  /** Hàm cũ, vẫn giữ nếu chỗ khác còn dùng */
  getAll(query) {
    return http.get('/api/v1/posts', { params: query });
  },

  /** GET /api/v1/posts/{id} -> PostDetailDto (Question detail) */
  getById(id) {
    return http.get(`/api/v1/posts/${id}`);
  },

  /** POST /api/v1/posts -> PostDetailDto (Create question) */
  create(body) {
    return http.post('/api/v1/posts', body);
  },

  /** PUT /api/v1/posts/{id} -> PostDetailDto (Update question) */
  update(id, body) {
    return http.put(`/api/v1/posts/${id}`, body);
  },

  /** POST /api/v1/posts/{id}/vote (Vote question) */
  vote(id, value) {
    return http.post(`/api/v1/posts/${id}/vote`, { value });
  },

  /**
   * GET /api/v1/tags/{tag}/posts?page=1&pageSize=10&sort=newest
   * Câu hỏi theo tag có phân trang
   */
  getByTag(tag, { page = 1, pageSize = 10, sort = 'newest' } = {}) {
    const params = { page, pageSize, sort };

    return http.get(`/api/v1/tags/${encodeURIComponent(tag)}/posts`, {
      params,
    });
  },
};
