// assets/js/api/revisions.api.js
import { http } from '../core/http.js';

export const postRevisionsApi = {
  list(postId, { page = 1, pageSize = 10 } = {}) {
    const p = new URLSearchParams({ page, pageSize });
    return http.get(`/posts/${postId}/revisions?${p.toString()}`); // -> {page,pageSize,total,items:RevisionDto[]}
  },
  get(postId, revId) {
    return http.get(`/posts/${postId}/revisions/${revId}`); // -> RevisionDetailDto
  },
};

export const commentRevisionsApi = {
  list(postId, commentId, { page = 1, pageSize = 10 } = {}) {
    const p = new URLSearchParams({ page, pageSize });
    return http.get(`/posts/${postId}/comments/${commentId}/revisions?${p.toString()}`);
  },
  get(postId, commentId, revId) {
    return http.get(`/posts/${postId}/comments/${commentId}/revisions/${revId}`);
  },
};
