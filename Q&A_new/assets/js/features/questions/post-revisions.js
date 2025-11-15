// assets/js/features/questions/post-revisions.js
import { postRevisionsApi } from '../../api/revisions.api.js';

function esc(s=''){return s.replace(/[&<>"']/g,m=>({ '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;' }[m]));}

export function mountPostRevisions(host, postId) {
  if (!host) return;

  host.innerHTML = `
    <div class="rev">
      <div class="rev__head">
        <div class="rev__title">Lịch sử sửa bài</div>
        <button class="c-btn" data-role="close">Đóng</button>
      </div>
      <div class="rev__list" data-bind="list">Đang tải…</div>
      <div class="q-pager" data-bind="pager"></div>
      <div class="rev__detail" data-bind="detail" hidden></div>
    </div>
  `;

  const listEl = host.querySelector('[data-bind="list"]');
  const pagerEl = host.querySelector('[data-bind="pager"]');
  const detailEl = host.querySelector('[data-bind="detail"]');

  host.querySelector('[data-role="close"]').onclick = () => host.hidden = true;

  let state = { page: 1, pageSize: 10, total: 0 };

  async function load() {
    listEl.innerHTML = `Đang tải…`;
    detailEl.hidden = true; detailEl.innerHTML = '';
    const res = await postRevisionsApi.list(postId, state);
    state.total = res.total || 0;
    const items = res.items || [];
    if (!items.length){ listEl.innerHTML = `<div class="empty">Chưa có bản sửa</div>`; pagerEl.innerHTML=''; return; }

    listEl.innerHTML = items.map(r => `
      <div class="rev__item" data-id="${r.id}">
        <div class="rev__meta">
          <b>#${r.id}</b> • ${esc(r.editorDisplayName || 'Ẩn danh')} • ${new Date(r.createdAt).toLocaleString()}
        </div>
        <div class="rev__sum">${esc(r.summary || '')}</div>
        <button class="c-btn" data-role="view" data-id="${r.id}">Xem diff</button>
      </div>
    `).join('');

    listEl.querySelectorAll('[data-role="view"]').forEach(btn=>{
      btn.onclick = async () => {
        const rev = await postRevisionsApi.get(postId, btn.dataset.id);
        detailEl.hidden = false;
        detailEl.innerHTML = `
          <div class="rev__detail-head">#${rev.id} — ${esc(rev.editorDisplayName||'')} • ${new Date(rev.createdAt).toLocaleString()}</div>
          ${rev.summary ? `<div class="rev__summary">Tóm tắt: ${esc(rev.summary)}</div>`:''}
          <div class="rev__diff">
            <div class="rev__col">
              <div class="rev__label">Tiêu đề (trước)</div>
              <pre class="rev__pre">${esc(rev.beforeTitle||'')}</pre>
              <div class="rev__label">Nội dung (trước)</div>
              <pre class="rev__pre">${esc((rev.beforeBody||'').replaceAll('\r',''))}</pre>
            </div>
            <div class="rev__col">
              <div class="rev__label">Tiêu đề (sau)</div>
              <pre class="rev__pre">${esc(rev.afterTitle||'')}</pre>
              <div class="rev__label">Nội dung (sau)</div>
              <pre class="rev__pre">${esc((rev.afterBody||'').replaceAll('\r',''))}</pre>
            </div>
          </div>
        `;
        detailEl.scrollIntoView({behavior:'smooth', block:'center'});
      };
    });

    const totalPages = Math.max(1, Math.ceil(state.total/state.pageSize));
    pagerEl.innerHTML = `
      <div class="pager">
        <button class="c-btn pager__btn" ${state.page<=1?'disabled':''} data-role="prev">‹ Trước</button>
        <span class="pager__info">Trang ${state.page} / ${totalPages}</span>
        <button class="c-btn pager__btn" ${state.page>=totalPages?'disabled':''} data-role="next">Sau ›</button>
      </div>`;
    pagerEl.querySelector('[data-role="prev"]')?.addEventListener('click', ()=>{ if(state.page>1){state.page--;load();} });
    pagerEl.querySelector('[data-role="next"]')?.addEventListener('click', ()=>{ const tp=Math.max(1,Math.ceil(state.total/state.pageSize)); if(state.page<tp){state.page++;load();} });
  }

  load();
}
