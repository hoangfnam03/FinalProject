// assets/js/api/comments.api.js
import { http } from '../core/http.js';

export const commentsApiExtra = {
  update(postId, commentId, { body, summary }) {
    return http.put(`/posts/${postId}/comments/${commentId}`, { body, summary }); // -> CommentDto
  },
};
