// assets/js/core/http.js
import { ENV } from './env.js';
import { storage } from './storage.js';

async function request(path, { method = 'GET', headers = {}, body } = {}) {
  const url = `${ENV.API_BASE_URL}${ENV.API_PREFIX}${path}`;
  const auth = storage.getAuth();

  const defaultHeaders = {
    'Content-Type': 'application/json',
  };

  // Nếu đã đăng nhập thì kèm Bearer
  if (auth?.accessToken) {
    defaultHeaders['Authorization'] = `Bearer ${auth.accessToken}`;
  }

  const res = await fetch(url, {
    method,
    headers: { ...defaultHeaders, ...headers },
    body: body ? JSON.stringify(body) : undefined,
    credentials: 'include', // nếu BE có set cookie (không ảnh hưởng bearer)
  });

  const isJson = res.headers.get('content-type')?.includes('application/json');
  const data = isJson ? await res.json().catch(() => ({})) : null;

  if (!res.ok) {
    const message = data?.message || data?.error || `HTTP ${res.status}`;
    const err = new Error(message);
    err.status = res.status;
    err.payload = data;
    throw err;
  }

  return data;
}
// THÊM bên cạnh postForm
export const http = {
  get: (p) => request(p, { method: 'GET' }),
  post: (p, body) => request(p, { method: 'POST', body }),
  put:  (p, body) => request(p, { method: 'PUT', body }),
  del:  (p) => request(p, { method: 'DELETE' }),

  postForm: async (path, formData) => {
    const url = `${ENV.API_BASE_URL}${ENV.API_PREFIX}${path}`;
    const auth = storage.getAuth();
    const headers = {};
    if (auth?.accessToken) headers['Authorization'] = `Bearer ${auth.accessToken}`;
    const res = await fetch(url, { method: 'POST', headers, body: formData, credentials: 'include' });
    const isJson = res.headers.get('content-type')?.includes('application/json');
    const data = isJson ? await res.json().catch(()=>({})) : null;
    if (!res.ok) { const err = new Error(data?.message || `HTTP ${res.status}`); err.status=res.status; err.payload=data; throw err; }
    return data;
  },

  // ✅ PUT multipart
  putForm: async (path, formData) => {
    const url = `${ENV.API_BASE_URL}${ENV.API_PREFIX}${path}`;
    const auth = storage.getAuth();
    const headers = {};
    if (auth?.accessToken) headers['Authorization'] = `Bearer ${auth.accessToken}`;
    const res = await fetch(url, { method: 'PUT', headers, body: formData, credentials: 'include' });
    const isJson = res.headers.get('content-type')?.includes('application/json');
    const data = isJson ? await res.json().catch(()=>({})) : null;
    if (!res.ok) { const err = new Error(data?.message || `HTTP ${res.status}`); err.status=res.status; err.payload=data; throw err; }
    return data;
  },
};
