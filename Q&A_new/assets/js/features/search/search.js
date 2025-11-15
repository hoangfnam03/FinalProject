// assets/js/features/search/search.js
import { auth } from '../../core/auth.js';
import { questionsApi } from '../../api/questions.api.js';

function getParams() {
  const usp = new URLSearchParams(location.search);
  return {
    q: usp.get('q')?.trim() || '',
    page: Number(usp.get('page') || '1'),
    pageSize: Number(usp.get('pageSize') || '10'),
  };
}
function setParams({ q, page, pageSize }, replace = false) {
  const url = new URL(location.href);
  if (q) url.searchParams.set('q', q); else url.searchParams.delete('q');
  url.searchParams.set('page', String(page || 1));
  url.searchParams.set('pageSize', String(pageSize || 10));
  if (replace) history.replaceState(null, '', url.toString());
  else history.pushState(null, '', url.toString());
}
function debounce(fn, ms = 300) {
  let t; return (...args) => { clearTimeout(t); t = setTimeout(() => fn(...args), ms); };
}
function escapeHtml(s) {
  return s.replace(/[&<>"']/g, m => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[m]));
}
function highlight(text, q) {
  if (!q) return escapeHtml(text || '');
  try {
    const esc = q.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    return escapeHtml(text || '').replace(new RegExp(esc, 'ig'), m => `<mark class="s-hl">${m}</mark>`);
  } catch { return escapeHtml(text || ''); }
}
function formatDate(iso) {
  if (!iso) return '';
  const d = new Date(iso); return d.toLocaleString();
}

export function initSearchPage() {
  // Guard
  if (!auth.isAuthenticated()) {
    window.location.href = '../auth/login.html';
    return;
  }

  const listHost = document.querySelector('[data-mount="list"]');
  const pagerHost = document.querySelector('[data-mount="pager"]');
  const input = document.querySelector('[data-role="search"]');
  const goBtn = document.querySelector('[data-role="go"]');

  let state = getParams();
  input.value = state.q;

  async function load() {
    listHost.innerHTML = `<div class="loading">Đang tìm kiếm…</div>`;
    try {
      const res = await questionsApi.list({ page: state.page, pageSize: state.pageSize, q: state.q });
      if (!res.items?.length) {
        listHost.innerHTML = `<div class="empty">Không tìm thấy kết quả cho “${escapeHtml(state.q)}”.</div>`;
        pagerHost.innerHTML = '';
        return;
      }

      const html = res.items.map(it => `
        <article class="q-item">
          <div class="q-item__stats">
            <div class="q-stat"><div class="q-stat__num">${it.votes ?? 0}</div><div class="q-stat__label">votes</div></div>
            <div class="q-stat"><div class="q-stat__num">${(it.tags || []).length}</div><div class="q-stat__label">tags</div></div>
          </div>
          <div class="q-item__main">
            <a class="q-item__title" href="../question/question-detail.html?id=${it.id}">${highlight(it.title ?? '(Không tiêu đề)', state.q)}</a>
            <div class="q-item__meta">
              <span class="q-meta__author">${it.authorName ?? 'Ẩn danh'}</span>
              <span class="q-meta__dot">•</span>
              <time class="q-meta__time" datetime="${it.createdAt || ''}">${formatDate(it.createdAt)}</time>
            </div>
            ${it.preview ? `<p class="q-item__preview">${highlight(it.preview, state.q)}</p>` : ``}
          </div>
        </article>
      `).join('');
      listHost.innerHTML = html;

      // pager
      const totalPages = Math.max(1, Math.ceil((res.total ?? 0) / res.pageSize));
      pagerHost.innerHTML = `
        <div class="pager">
          <button class="c-btn pager__btn" data-role="prev" ${res.page<=1?'disabled':''}>‹ Trước</button>
          <span class="pager__info">Trang ${res.page} / ${totalPages}</span>
          <button class="c-btn pager__btn" data-role="next" ${res.page>=totalPages?'disabled':''}>Sau ›</button>
        </div>
      `;
      pagerHost.querySelector('[data-role="prev"]')?.addEventListener('click', () => {
        if (state.page > 1) {
          state.page--; setParams(state); load();
        }
      });
      pagerHost.querySelector('[data-role="next"]')?.addEventListener('click', () => {
        const tp = Math.max(1, Math.ceil((res.total ?? 0) / res.pageSize));
        if (state.page < tp) {
          state.page++; setParams(state); load();
        }
      });

    } catch (err) {
      console.error(err);
      listHost.innerHTML = `<div class="c-alert is-visible">Lỗi tìm kiếm: ${escapeHtml(err?.payload?.message || err.message || 'Không xác định')}</div>`;
      pagerHost.innerHTML = '';
    }
  }

  const doSearch = () => {
    state.q = input.value.trim();
    state.page = 1;
    setParams(state);
    load();
  };
  const debounced = debounce(doSearch, 300);

  input.addEventListener('input', debounced);
  input.addEventListener('keydown', (e) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      doSearch();
    }
  });
  goBtn.addEventListener('click', doSearch);

  // Back/forward giữ trạng thái
  window.addEventListener('popstate', () => {
    state = getParams();
    input.value = state.q;
    load();
  });

  // tải lần đầu
  setParams(state, /*replace*/ true);
  load();
}
