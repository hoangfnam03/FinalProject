// assets/js/features/questions/question-detail.js
import { auth } from '../../core/auth.js';
import { postDetailApi, commentsApi } from '../../api/post-detail.api.js';

function qs(name) {
  const m = new URLSearchParams(location.search);
  return m.get(name);
}
function formatDate(iso) {
  if (!iso) return '';
  const d = new Date(iso);
  return d.toLocaleString();
}
function setLoading(el, on, textWhenOn = 'Đang gửi…', textWhenOff = 'Gửi bình luận') {
  if (!el) return;
  el.disabled = on;
  el.dataset.loading = on ? '1' : '0';
  el.textContent = on ? textWhenOn : textWhenOff;
}

export function initQuestionDetailPage() {
  // Guard
  if (!auth.isAuthenticated()) {
    window.location.href = '../auth/login.html';
    return;
  }

  const id = qs('id');
  if (!id) {
    alert('Thiếu id bài viết');
    window.location.href = './questions.html';
    return;
  }

  // Mounts
  const postHost = document.querySelector('[data-mount="post"]');
  const commentsHost = document.querySelector('[data-mount="comments"]');
  const pagerHost = document.querySelector('[data-mount="pager"]');
  const form = document.querySelector('#commentForm');
  const bodyInput = form?.querySelector('[name="body"]');
  const submitBtn = form?.querySelector('[data-role="submit"]');
  const alertEl = form?.querySelector('[data-role="alert"]');

  let cState = { page: 1, pageSize: 10, total: 0 };

  function renderPost(p) {
    postHost.innerHTML = `
      <header class="qd-head">
        <h1 class="qd-title">${p.title}</h1>
        <div class="qd-tags">
          ${(p.tags || []).map(t => `<span class="qd-tag">#${t}</span>`).join('')}
        </div>
        <div class="qd-meta">
          <span>${p.authorDisplayName || 'Ẩn danh'}</span>
          <span class="qd-dot">•</span>
          <time datetime="${p.createdAt}">${formatDate(p.createdAt)}</time>
        </div>
        <div class="qd-vote">
          <button class="c-btn qd-vote__btn" data-role="vote-up">▲</button>
          <div class="qd-vote__score" data-bind="score">${p.score ?? 0}</div>
          <button class="c-btn qd-vote__btn" data-role="vote-down">▼</button>
        </div>
      </header>

      <article class="qd-body">
        ${p.body ? `<p>${p.body.replace(/\n/g, '<br>')}</p>` : '<em>(Không có nội dung)</em>'}
      </article>
    `;

    // wire vote
    const scoreEl = postHost.querySelector('[data-bind="score"]');
    postHost.querySelector('[data-role="vote-up"]')?.addEventListener('click', async () => {
      try {
        const res = await postDetailApi.votePost(id, +1);
        if (typeof res?.score === 'number') scoreEl.textContent = res.score;
      } catch (e) { console.warn(e); }
    });
    postHost.querySelector('[data-role="vote-down"]')?.addEventListener('click', async () => {
      try {
        const res = await postDetailApi.votePost(id, -1);
        if (typeof res?.score === 'number') scoreEl.textContent = res.score;
      } catch (e) { console.warn(e); }
    });
  }

function renderCommentsPaged(paged) {
  cState.total = paged.total ?? 0;
  commentsHost.innerHTML = (paged.items ?? []).map(c => `
    <div class="cmt" data-id="${c.id}">
      <div class="cmt__meta">
        <span class="cmt__author">${c.authorDisplayName || 'Ẩn danh'}</span>
        <span class="cmt__dot">•</span>
        <time class="cmt__time" datetime="${c.createdAt}">${formatDate(c.createdAt)}</time>
      </div>
      <div class="cmt__body">${(c.body || '').replace(/\n/g, '<br>')}</div>
      <div class="cmt__actions">
        <button class="c-btn cmt__btn" data-role="cmt-history">Lịch sử</button>
        <button class="c-btn cmt__btn" data-role="cmt-edit">Sửa</button>
      </div>
      <div class="cmt__revpanel" hidden></div>
      <div class="cmt__editpanel" hidden>
        <textarea class="c-input" rows="4" data-role="edit-body">${c.body || ''}</textarea>
        <input class="c-input u-mt-8" placeholder="Tóm tắt thay đổi (không bắt buộc)" data-role="edit-summary">
        <div class="u-mt-8">
          <button class="c-btn c-btn--primary" data-role="edit-save">Lưu</button>
          <button class="c-btn" data-role="edit-cancel">Huỷ</button>
        </div>
      </div>
    </div>
  `).join('') || `<div class="empty">Chưa có bình luận</div>`;

  // Wire buttons
  commentsHost.querySelectorAll('.cmt').forEach(card => {
    const cid = card.dataset.id;
    const revPanel = card.querySelector('.cmt__revpanel');
    const editPanel = card.querySelector('.cmt__editpanel');
    // Lịch sử
    card.querySelector('[data-role="cmt-history"]').onclick = async () => {
      revPanel.hidden = !revPanel.hidden;
      if (!revPanel.hidden) {
        revPanel.innerHTML = 'Đang tải…';
        const res = await commentsApi.list(id, cid, { page:1, pageSize:10 });
        const items = res.items || [];
        revPanel.innerHTML = items.length ? items.map(r=>`
          <div class="rev__item">
            <div class="rev__meta"><b>#${r.id}</b> • ${r.editorDisplayName||''} • ${new Date(r.createdAt).toLocaleString()}</div>
            <div class="rev__sum">${esc(r.summary||'')}</div>
            <button class="c-btn" data-role="view" data-id="${r.id}">Xem</button>
          </div>
        `).join('') : `<div class="empty">Chưa có bản sửa</div>`;

        revPanel.querySelectorAll('[data-role="view"]').forEach(btn=>{
          btn.onclick = async () => {
            const d = await commentRevisionsApi.get(id, cid, btn.dataset.id);
            const html = `
              <div class="rev__detail u-mt-8">
                <div class="rev__detail-head">#${d.id} — ${d.editorDisplayName||''} • ${new Date(d.createdAt).toLocaleString()}</div>
                ${d.summary?`<div class="rev__summary">Tóm tắt: ${esc(d.summary)}</div>`:''}
                <div class="rev__diff">
                  <div class="rev__col">
                    <div class="rev__label">Trước</div>
                    <pre class="rev__pre">${esc((d.beforeBody||'').replaceAll('\r',''))}</pre>
                  </div>
                  <div class="rev__col">
                    <div class="rev__label">Sau</div>
                    <pre class="rev__pre">${esc((d.afterBody||'').replaceAll('\r',''))}</pre>
                  </div>
                </div>
              </div>`;
            revPanel.insertAdjacentHTML('beforeend', html);
          };
        });
      }
    };
    // Sửa
    card.querySelector('[data-role="cmt-edit"]').onclick = () => { editPanel.hidden = !editPanel.hidden; };
    card.querySelector('[data-role="edit-cancel"]').onclick = () => { editPanel.hidden = true; };
    card.querySelector('[data-role="edit-save"]').onclick = async () => {
      const body = editPanel.querySelector('[data-role="edit-body"]').value.trim();
      const summary = editPanel.querySelector('[data-role="edit-summary"]').value.trim();
      if (!body){ alert('Nhập nội dung.'); return; }
      try {
        await commentsApiExtra.update(id, cid, { body, summary: summary || null });
        editPanel.hidden = true;
        loadComments();
      } catch (e) { alert('Lưu thất bại.'); }
    };
  });

  // pager như cũ...
  const totalPages = Math.max(1, Math.ceil(cState.total / cState.pageSize));
  pagerHost.innerHTML = `
    <div class="pager">
      <button class="c-btn pager__btn" data-role="prev" ${cState.page<=1?'disabled':''}>‹ Trước</button>
      <span class="pager__info">Trang ${cState.page} / ${totalPages}</span>
      <button class="c-btn pager__btn" data-role="next" ${cState.page>=totalPages?'disabled':''}>Sau ›</button>
    </div>`;
    pagerHost.querySelector('[data-role="prev"]')?.addEventListener('click', () => {
      if (cState.page > 1) { cState.page--; loadComments(); }
    });
    pagerHost.querySelector('[data-role="next"]')?.addEventListener('click', () => {
      const tp = Math.max(1, Math.ceil(cState.total / cState.pageSize));
      if (cState.page < tp) { cState.page++; loadComments(); }
    });
  }

  async function loadPost() {
    postHost.innerHTML = `<div class="loading">Đang tải bài viết…</div>`;
    try {
      const p = await postDetailApi.getById(id);
      renderPost(p);
    } catch (e) {
      console.error(e);
      postHost.innerHTML = `<div class="c-alert is-visible">Không tải được bài viết.</div>`;
    }
  }

  async function loadComments() {
    commentsHost.innerHTML = `<div class="loading">Đang tải bình luận…</div>`;
    try {
      const paged = await commentsApi.list(id, { page: cState.page, pageSize: cState.pageSize });
      renderCommentsPaged(paged);
    } catch (e) {
      console.error(e);
      commentsHost.innerHTML = `<div class="c-alert is-visible">Không tải được bình luận.</div>`;
      pagerHost.innerHTML = '';
    }
  }

  // Submit comment
  form?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const body = bodyInput?.value?.trim();
    if (!body) {
      alertEl?.classList.add('is-visible'); 
      alertEl.textContent = 'Hãy nhập nội dung bình luận.';
      return;
    }
    alertEl?.classList.remove('is-visible');

    try {
      setLoading(submitBtn, true);
      await commentsApi.create(id, { body });
      bodyInput.value = '';
      cState.page = 1;   // quay về trang 1 để thấy comment mới
      await loadComments();
    } catch (err) {
      console.error(err);
      alertEl?.classList.add('is-visible');
      alertEl.textContent = err?.payload?.message || err.message || 'Gửi bình luận thất bại.';
    } finally {
      setLoading(submitBtn, false);
    }
  });

  // boot
  loadPost();
  loadComments();
}
