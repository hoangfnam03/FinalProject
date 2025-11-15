// assets/js/api/post-detail.api.js
import { http } from '../core/http.js';

export const postDetailApi = {
  async getById(id) {
    // GET /api/v1/posts/{id} -> PostDetailDto
    return http.get(`/posts/${id}`);
  },
  async votePost(id, value) {
    // POST /api/v1/posts/{id}/vote  { value }
    return http.post(`/posts/${id}/vote`, { value });
  },
};

export const commentsApi = {
  async list(postId, { page = 1, pageSize = 10 } = {}) {
    const params = new URLSearchParams({ page, pageSize });
    return http.get(`/posts/${postId}/comments?${params.toString()}`);
  },
  async create(postId, { body, parentCommentId = null } = {}) {
    // POST /api/v1/posts/{postId}/comments { body, parentCommentId? }
    return http.post(`/posts/${postId}/comments`, { body, parentCommentId });
  },
  async vote(postId, commentId, value) {
    // POST /api/v1/posts/{postId}/comments/{commentId}/vote { value }
    return http.post(`/posts/${postId}/comments/${commentId}/vote`, { value });
  },
};
