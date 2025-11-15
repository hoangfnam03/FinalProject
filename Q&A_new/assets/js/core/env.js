// assets/js/core/env.js
// Cấu hình môi trường đơn giản.
// Ưu tiên đọc từ <meta name="api-base-url" content="..."> nếu có.
export const ENV = (() => {
  const meta = document.querySelector('meta[name="api-base-url"]');
  const API_BASE_URL = (meta && meta.content) || 'http://localhost:7006';
  // Luôn không có slash cuối
  const normalized = API_BASE_URL.replace(/\/+$/, '');
  return {
    API_BASE_URL: normalized,
    API_PREFIX: '/api/v1',
  };
})();
