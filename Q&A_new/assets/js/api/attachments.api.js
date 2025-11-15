// assets/js/api/attachments.api.js
import { http } from '../core/http.js';

export const postAttachmentsApi = {
  list(postId){ return http.get(`/posts/${postId}/attachments`); },
  uploadImage(postId, file, caption=null){
    const fd=new FormData(); fd.append('file', file); if(caption) fd.append('caption', caption);
    return http.postForm(`/posts/${postId}/attachments/images`, fd);
  },
  addLink(postId, { linkUrl, displayText }){ return http.post(`/posts/${postId}/attachments/links`, { linkUrl, displayText }); },
  remove(postId, attId){ return http.del(`/posts/${postId}/attachments/${attId}`); },
};

export const commentAttachmentsApi = {
  list(postId, commentId){ return http.get(`/posts/${postId}/comments/${commentId}/attachments`); },
  uploadImage(postId, commentId, file, caption=null){
    const fd=new FormData(); fd.append('file', file); if(caption) fd.append('caption', caption);
    return http.postForm(`/posts/${postId}/comments/${commentId}/attachments/images`, fd);
  },
  addLink(postId, commentId, { linkUrl, displayText }){
    return http.post(`/posts/${postId}/comments/${commentId}/attachments/links`, { linkUrl, displayText });
  },
  remove(postId, commentId, attId){ return http.del(`/posts/${postId}/comments/${commentId}/attachments/${attId}`); },
};
