// assets/js/entry/notifications.entry.js
import { auth } from '../core/auth.js';
import { notificationsApi } from '../api/notifications.api.js';
import { initBell } from '../features/notifications/bell.js';

function getParams() {
  const u = new URLSearchParams(location.search);
  return { page: Number(u.get('page') || '1'), pageSize: Number(u.get('pageSize') || '20') };
}
function setParams(s, replace = false) {
  const url = new URL(location.href);
  url.searchParams.set('page', s.page);
  url.searchParams.set('pageSize', s.pageSize);
  if (replace) history.replaceState(null, '', url.toString());
  else history.pushState(null, '', url.toString());
}

document.addEventListener('DOMContentLoaded', async () => {
  if (!auth.isAuthenticated()) { window.location.href = '../auth/login.html'; return; }

  const bellHost = document.querySelector('[data-mount="notif"]');
  initBell(bellHost);

  const listHost = document.querySelector('[data-mount="list"]');
  const pager = document.querySelector('[data-mount="pager"]');
  const markAll = document.querySelector('[data-role="mark-all"]');
  let state = getParams();

  async function load() {
    listHost.innerHTML = `<div class="loading">Đang tải…</div>`;
    try {
      const res = await notificationsApi.list(state);
      const items = res.items ?? [];
      if (!items.length) {
        listHost.innerHTML = `<div class="empty">Không có thông báo.</div>`;
        pager.innerHTML = '';
        return;
      }
      listHost.innerHTML = items.map(it => `
        <div class="nf-item ${it.isRead ? '' : 'is-unread'}" data-id="${it.id}">
          <div class="nf-item__text">${it.text ?? it.title ?? '(Thông báo)'}</div>
          <div class="nf-item__meta">
            <time>${new Date(it.createdAt).toLocaleString()}</time>
            ${it.link ? `<a href="${it.link}">Mở</a>` : ''}
            <a href="#" data-role="read">Đánh dấu đã đọc</a>
          </div>
        </div>
      `).join('');

      listHost.querySelectorAll('[data-role="read"]').forEach(a => {
        a.addEventListener('click', async (e) => {
          e.preventDefault();
          const card = a.closest('.nf-item');
          try { await notificationsApi.markRead(card.dataset.id); card.classList.remove('is-unread'); }
          catch {}
        });
      });

      const totalPages = Math.max(1, Math.ceil((res.total ?? 0) / res.pageSize));
      pager.innerHTML = `
        <div class="pager">
          <button class="c-btn pager__btn" data-role="prev" ${res.page<=1?'disabled':''}>‹ Trước</button>
          <span class="pager__info">Trang ${res.page} / ${totalPages}</span>
          <button class="c-btn pager__btn" data-role="next" ${res.page>=totalPages?'disabled':''}>Sau ›</button>
        </div>
      `;
      pager.querySelector('[data-role="prev"]')?.addEventListener('click', () => { if (state.page>1) { state.page--; setParams(state); load(); }});
      pager.querySelector('[data-role="next"]')?.addEventListener('click', () => {
        const tp = Math.max(1, Math.ceil((res.total ?? 0) / res.pageSize));
        if (state.page<tp) { state.page++; setParams(state); load(); }
      });

    } catch (e) {
      listHost.innerHTML = `<div class="c-alert is-visible">Không tải được danh sách.</div>`;
      pager.innerHTML = '';
    }
  }

  markAll.addEventListener('click', async () => {
    try { await notificationsApi.markAllRead(); load(); }
    catch {}
  });

  setParams(state, true);
  load();
});
