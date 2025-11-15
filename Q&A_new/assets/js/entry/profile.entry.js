import { initProfilePage } from '../features/user/profile.js';
import { initBell } from '../features/notifications/bell.js';
import { meApi, usersApi } from '../api/me.api.js';
import { auth } from '../core/auth.js';

document.addEventListener('DOMContentLoaded', async () => {
  initProfilePage();
  initBell(document.querySelector('[data-mount="notif"]'));

  // load Me để fill form & activities
  try {
    const me = await meApi.get();
    const form = document.getElementById('profileForm');
    form.querySelector('[name="displayName"]').value = me.displayName || '';
    form.querySelector('[name="bio"]').value = me.bio || '';
    // activities (dùng users/{id})
    const act = await usersApi.activities(me.id, { page:1, pageSize:20 });
    const list = (act.items || []).map(a => `
      <div class="nf-item">
        <div class="nf-item__text">[${a.type}] ${a.postTitle || a.excerpt || ''}</div>
        <div class="nf-item__meta"><time>${new Date(a.createdAt).toLocaleString()}</time></div>
      </div>
    `).join('') || '<div class="empty">Chưa có hoạt động.</div>';
    document.getElementById('activities').innerHTML = list;

    // submit update
    form.addEventListener('submit', async (e) => {
      e.preventDefault();
      const alert = form.querySelector('[data-role="alert"]');
      try {
        await meApi.update({
          displayName: form.querySelector('[name="displayName"]').value.trim() || null,
          bio: form.querySelector('[name="bio"]').value.trim() || null,
        });
        alert.classList.add('is-visible'); alert.textContent = 'Đã lưu.';
      } catch (err) {
        alert.classList.add('is-visible'); alert.textContent = err?.payload?.message || 'Lỗi lưu.';
      }
    });

    // upload avatar
    document.getElementById('btnUploadAvatar').onclick = async () => {
      const file = document.getElementById('avatarFile').files?.[0];
      const msg = document.getElementById('avatarMsg');
      if (!file){ msg.classList.add('is-visible'); msg.textContent='Chọn ảnh trước.'; return; }
      try { const r = await meApi.uploadAvatar(file); msg.classList.add('is-visible'); msg.textContent='Tải lên OK.'; }
      catch(e){ msg.classList.add('is-visible'); msg.textContent='Tải lên lỗi.'; }
    };

  } catch { /* ignore */ }
});
