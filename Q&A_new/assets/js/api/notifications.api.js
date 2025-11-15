// assets/js/api/notifications.api.js
import { http } from '../core/http.js';

function qs({ page = 1, pageSize = 10 } = {}) {
  const p = new URLSearchParams({ page, pageSize });
  return `?${p.toString()}`;
}

export const notificationsApi = {
  list({ page = 1, pageSize = 10 } = {}) {
    return http.get(`/notifications${qs({ page, pageSize })}`);
  },
  unreadCount() {
    return http.get('/notifications/unread-count'); // { count }
  },
  markRead(id) {
    return http.post(`/notifications/${id}/read`, {});
  },
  markAllRead() {
    return http.post('/notifications/read-all', {});
  },
};
