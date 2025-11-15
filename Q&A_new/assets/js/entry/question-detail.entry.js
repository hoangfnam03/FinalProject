import { initQuestionDetailPage } from '../features/questions/question-detail.js';
import { mountPostRevisions } from '../features/questions/post-revisions.js';
import { initBell } from '../features/notifications/bell.js';

document.addEventListener('DOMContentLoaded', () => {
  initQuestionDetailPage();

  const host = document.querySelector('[data-mount="notif"]');
  initBell(host);

  const btn = document.getElementById('btnRevisions');
  const panel = document.getElementById('revPanel');
  // lấy id từ URL
  const id = new URLSearchParams(location.search).get('id');
  btn?.addEventListener('click', () => {
    panel.hidden = !panel.hidden;
    if (!panel.hidden) mountPostRevisions(panel, id);
  });
  const postId = new URLSearchParams(location.search).get('id');
  const list = document.getElementById('postAttList');

  async function loadAtt() {
    try {
      const arr = await postAttachmentsApi.list(postId);
      list.innerHTML = arr.length ? arr.map(a => `
        <div class="att" data-id="${a.id}">
          ${a.type==='image' ? `<img src="${a.url}" alt="${a.caption||''}" style="max-width:160px;border-radius:8px">` : `
          <a href="${a.linkUrl}" target="_blank">${a.displayText||a.linkUrl}</a>`}
          <button class="c-btn" data-role="del">Xoá</button>
        </div>
      `).join('') : '<div class="empty">Chưa có đính kèm.</div>';
      list.querySelectorAll('[data-role="del"]').forEach(btn=>{
        btn.onclick = async ()=>{ await postAttachmentsApi.remove(postId, btn.closest('.att').dataset.id); loadAtt(); };
      })
    } catch { list.innerHTML = '<div class="c-alert is-visible">Không tải được đính kèm.</div>'; }
  }
  loadAtt();

  document.getElementById('btnPostImg').onclick = async () => {
    const f = document.getElementById('postAttImage').files?.[0];
    const cap = document.getElementById('postAttCaption').value.trim()||null;
    if (!f) return alert('Chọn ảnh');
    await postAttachmentsApi.uploadImage(postId, f, cap); loadAtt();
  };
  document.getElementById('btnPostLink').onclick = async () => {
    const url = document.getElementById('postLinkUrl').value.trim();
    const text = document.getElementById('postLinkText').value.trim() || null;
    if (!url) return alert('Nhập URL');
    await postAttachmentsApi.addLink(postId, { linkUrl: url, displayText: text }); loadAtt();
  };
});
