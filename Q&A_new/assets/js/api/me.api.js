import { http } from '../core/http.js';
export const meApi = {
  get(){ return http.get('/me'); },
  update(p){ return http.put('/me', p); },

  // ✅ dùng PUT + multipart, field name phải là "file"
  uploadAvatar(file){
    const fd = new FormData();
    fd.append('file', file);
    return http.putForm('/me/avatar', fd);
  },
};
export const usersApi = {
  getPublic(id){ return http.get(`/users/${id}`); },
  activities(id, {page=1,pageSize=20}={}){ const p=new URLSearchParams({page,pageSize}); return http.get(`/users/${id}/activities?${p}`); },
};
