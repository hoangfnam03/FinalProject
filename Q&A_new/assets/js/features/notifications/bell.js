// assets/js/features/notifications/bell.js
import { notificationsApi } from '../../api/notifications.api.js';
import { auth } from '../../core/auth.js';

export function initBell(host) {
  if (!host) return;
  if (!auth.isAuthenticated()) return;

  host.innerHTML = `
    <div class="nbell">
      <button class="nbell__btn" data-role="toggle" aria-label="Notifications">
        🔔 <span class="nbell__badge" data-bind="badge" hidden>0</span>
      </button>
      <div class="nbell__panel" data-role="panel" hidden>
        <div class="nbell__head">
          <div class="nbell__title">Thông báo</div>
          <button class="c-btn nbell__markall" data-role="mark-all">Đánh dấu đã đọc</button>
        </div>
        <div class="nbell__list" data-bind="list">Đang tải…</div>
        <div class="nbell__foot">
          <a class="c-btn" href="../notifications/notifications.html">Xem tất cả</a>
        </div>
      </div>
    </div>
  `;

  const toggleBtn = host.querySelector('[data-role="toggle"]');
  const panel     = host.querySelector('[data-role="panel"]');
  const listEl    = host.querySelector('[data-bind="list"]');
  const badgeEl   = host.querySelector('[data-bind="badge"]');
  const markAll   = host.querySelector('[data-role="mark-all"]');

  let open = false;
  let pageSize = 10;

  function updateBadge(n) {
    if (n > 0) {
      badgeEl.textContent = n;
      badgeEl.hidden = false;
    } else {
      badgeEl.hidden = true;
    }
  }

  async function refreshBadge() {
    try {
      const { count } = await notificationsApi.unreadCount();
      updateBadge(count || 0);
    } catch { /* no-op */ }
  }

  async function loadList() {
    listEl.innerHTML = 'Đang tải…';
    try {
      const res = await notificationsApi.list({ page: 1, pageSize });
      const items = res.items ?? [];
      if (!items.length) {
        listEl.innerHTML = `<div class="nbell__empty">Chưa có thông báo</div>`;
        return;
      }
      listEl.innerHTML = items.map(it => `
        <div class="nbell__item ${it.isRead ? '' : 'is-unread'}" data-id="${it.id}">
          <div class="nbell__text">${it.text ?? it.title ?? '(Thông báo)'}</div>
          <time class="nbell__time">${new Date(it.createdAt).toLocaleString()}</time>
          ${it.link ? `<a class="nbell__link" href="${it.link}">Mở</a>` : ''}
        </div>
      `).join('');

      // click item => mark read + mở link nếu có
      listEl.querySelectorAll('.nbell__item').forEach(el => {
        el.addEventListener('click', async () => {
          const id = el.dataset.id;
          try { await notificationsApi.markRead(id); el.classList.remove('is-unread'); refreshBadge(); }
          catch {}
          const a = el.querySelector('.nbell__link');
          if (a?.href) window.location.href = a.href;
        });
      });
    } catch (e) {
      listEl.innerHTML = `<div class="c-alert is-visible">Không tải được thông báo.</div>`;
    }
  }

  toggleBtn.addEventListener('click', () => {
    open = !open;
    panel.hidden = !open;
    if (open) loadList();
  });

  markAll.addEventListener('click', async () => {
    try {
      await notificationsApi.markAllRead();
      listEl.querySelectorAll('.nbell__item.is-unread').forEach(el => el.classList.remove('is-unread'));
      updateBadge(0);
    } catch {}
  });

  // đóng panel khi click bên ngoài
  document.addEventListener('click', (e) => {
    if (!open) return;
    if (!host.contains(e.target)) {
      open = false; panel.hidden = true;
    }
  });

  // lần đầu: nạp badge + auto refresh mỗi 60s
  refreshBadge();
  setInterval(refreshBadge, 60000);
}
