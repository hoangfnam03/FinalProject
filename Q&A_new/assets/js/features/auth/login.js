// assets/js/features/auth/login.js
import { authApi } from '../../api/auth.api.js';
import { auth } from '../../core/auth.js';

function setLoading(form, loading) {
  const btn = form.querySelector('[data-role="submit"]');
  if (!btn) return;
  btn.disabled = loading;
  btn.dataset.loading = loading ? '1' : '0';
  btn.innerText = loading ? 'Đang đăng nhập…' : 'Đăng nhập';
}

function showError(form, message) {
  const alert = form.querySelector('[data-role="alert"]');
  if (alert) {
    alert.classList.remove('d-none');
    alert.textContent = message;
  } else {
    alert?.remove?.();
    const div = document.createElement('div');
    div.className = 'alert alert-danger mt-3';
    div.setAttribute('data-role', 'alert');
    div.textContent = message;
    form.appendChild(div);
  }
}

export function setupLoginForm({ formSelector = '#loginForm', redirectTo = '/page/question/questions.html' } = {}) {
  const form = document.querySelector(formSelector);
  if (!form) return;

  // Nếu đã login => chuyển thẳng
  if (auth.isAuthenticated()) {
    window.location.href = redirectTo;
    return;
  }

  form.addEventListener('submit', async (e) => {
    e.preventDefault();
    const email = form.querySelector('input[name="email"]')?.value?.trim();
    const password = form.querySelector('input[name="password"]')?.value || '';

    // Validate đơn giản
    if (!email || !password) {
      showError(form, 'Vui lòng nhập email và mật khẩu.');
      return;
    }

    try {
      setLoading(form, true);
      const res = await authApi.login(email, password);
      // res là AuthResponse (accessToken, expiresIn, refreshToken)
      auth.loginSuccess(res);
      window.location.href = redirectTo;
    } catch (err) {
      console.error(err);
      // Thông báo chung; nếu BE có message cụ thể, ưu tiên message đó
      const msg = err?.payload?.message || err.message || 'Đăng nhập thất bại.';
      showError(form, msg);
    } finally {
      setLoading(form, false);
    }
  });
}
