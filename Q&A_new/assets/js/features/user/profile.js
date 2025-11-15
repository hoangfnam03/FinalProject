// assets/js/features/user/profile.js
import { auth } from '../../core/auth.js';
import { statsApi } from '../../api/stats.api.js';

function decodeJwt(token) {
  try {
    const payload = token.split('.')[1];
    const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(decodeURIComponent(escape(json)));
  } catch {
    return null;
  }
}

function initialsFrom(nameOrEmail) {
  const s = (nameOrEmail || '').trim();
  if (!s) return 'U';
  const parts = s.split(/[\s.@_-]+/).filter(Boolean);
  const first = (parts[0] || 'U')[0];
  const last  = (parts[parts.length - 1] || '')[0] || '';
  return (first + last).toUpperCase();
}

export function initProfilePage() {
  // Guard chưa đăng nhập
  if (!auth.isAuthenticated()) {
    window.location.href = '../auth/login.html';
    return;
  }

  // Mounts
  const headerHost = document.querySelector('[data-mount="profile-header"]');
  const statsHost  = document.querySelector('[data-mount="profile-stats"]');
  const actionsHost= document.querySelector('[data-mount="profile-actions"]');

  // Lấy claim từ JWT
  const token = auth.getAccessToken();
  const claims = token ? decodeJwt(token) : null;
  const displayName = claims?.name || claims?.given_name || claims?.displayName || '';
  const email = claims?.email || '';

  // Header (avatar chữ cái)
  const initials = initialsFrom(displayName || email);
  headerHost.innerHTML = `
    <div class="prof-head">
      <div class="prof-avatar" aria-hidden="true">${initials}</div>
      <div class="prof-id">
        <div class="prof-name">${displayName || '(Chưa đặt tên)'}</div>
        <div class="prof-email">${email || ''}</div>
      </div>
    </div>
  `;

  // Stats
  statsHost.innerHTML = `<div class="loading">Đang tải số liệu…</div>`;
  statsApi.getMyStats()
    .then(s => {
      statsHost.innerHTML = `
        <div class="stats">
          <div class="stat"><div class="stat__num">${s.questions ?? 0}</div><div class="stat__label">Câu hỏi</div></div>
          <div class="stat"><div class="stat__num">${s.comments ?? 0}</div><div class="stat__label">Bình luận</div></div>
          <div class="stat"><div class="stat__num">${s.votes ?? 0}</div><div class="stat__label">Votes</div></div>
          <div class="stat"><div class="stat__num">${s.reputation ?? 0}</div><div class="stat__label">Reputation</div></div>
        </div>
      `;
    })
    .catch(err => {
      console.error(err);
      statsHost.innerHTML = `<div class="c-alert is-visible">Không tải được số liệu.</div>`;
    });

  // Actions (tạm thời)
  actionsHost.innerHTML = `
    <div class="prof-actions">
      <a class="c-btn" href="../question/questions.html">Về danh sách câu hỏi</a>
      <a class="c-btn c-btn--primary" href="../question/post-create.html">Hỏi mới</a>
    </div>
  `;
}
