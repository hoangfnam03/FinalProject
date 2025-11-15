// assets/js/features/questions/questions.js
import { auth } from '../../core/auth.js';
import { storage } from '../../core/storage.js';
import { questionsApi } from '../../api/questions.api.js';
import { initBell } from '../notifications/bell.js';

// ==== Helpers ====
function decodeJwt(token) {
  try {
    const payload = token.split('.')[1];
    const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(decodeURIComponent(escape(json)));
  } catch {
    return null;
  }
}
function formatDate(iso) {
  if (!iso) return '';
  const d = new Date(iso);
  return d.toLocaleString();
}

// ==== Navbar ====
function renderNavbar(container) {
  const access = auth.getAccessToken();
  const claims = access ? decodeJwt(access) : null;
  const displayName = claims?.name || claims?.given_name || claims?.email || 'Tài khoản';

  container.innerHTML = `
    <div class="nav">
      <div class="nav__left">
        <a class="nav__brand" href="./questions.html">IntraQ&A</a>
        <a class="c-btn c-btn--primary nav__ask" href="./post-create.html">Hỏi mới</a>
      </div>
      <div class="nav__right">
        <div data-mount="notif"></div>  <!-- THÊM -->
        <a class="nav__profilelink" href="../user/profile.html" title="Hồ sơ">Hi, ${displayName}</a>
        <a class="c-btn" href="../user/profile.html">Hồ sơ</a>
        <button class="c-btn nav__logout" data-role="logout">Đăng xuất</button>
      </div>
    </div>
  `;
  // init bell
  const bellHost = container.querySelector('[data-mount="notif"]');
  initBell(bellHost);

  const logoutBtn = container.querySelector('[data-role="logout"]');
  logoutBtn?.addEventListener('click', () => {
    storage.clearAuth();
    window.location.href = '../auth/login.html';
  });
}

// ==== List & Pagination ====
function renderEmpty(container) {
  container.innerHTML = `
    <div class="empty">
      <div class="empty__title">Chưa có câu hỏi nào</div>
      <div class="empty__hint">Hãy là người đầu tiên đặt câu hỏi!</div>
    </div>
  `;
}

function renderError(container, message) {
  container.innerHTML = `<div class="c-alert is-visible">${message || 'Đã xảy ra lỗi.'}</div>`;
}

function renderLoading(container) {
  container.innerHTML = `<div class="loading">Đang tải…</div>`;
}

// ...giữ nguyên phần trên

function renderList(container, items) {
  const html = items.map(it => `
    <article class="q-item">
      <div class="q-item__stats">
        <div class="q-stat">
          <div class="q-stat__num">${it.votes ?? 0}</div>
          <div class="q-stat__label">votes</div>
        </div>
        <div class="q-stat">
          <div class="q-stat__num">${(it.tags || []).length}</div>
          <div class="q-stat__label">tags</div>
        </div>
      </div>
      <div class="q-item__main">
        <a class="q-item__title" href="./question-detail.html?id=${it.id}">${it.title ?? '(Không tiêu đề)'}</a>
        <div class="q-item__meta">
          <span class="q-meta__author">${it.authorName ?? 'Ẩn danh'}</span>
          <span class="q-meta__dot">•</span>
          <time class="q-meta__time" datetime="${it.createdAt || ''}">${formatDate(it.createdAt)}</time>
        </div>
        ${it.preview ? `<p class="q-item__preview">${it.preview}</p>` : ``}
      </div>
    </article>
  `).join('');
  container.innerHTML = html;
}


function renderPagination(container, { page, pageSize, total }) {
  const totalPages = Math.max(1, Math.ceil((total ?? 0) / pageSize));
  const canPrev = page > 1;
  const canNext = page < totalPages;

  container.innerHTML = `
    <div class="pager">
      <button class="c-btn pager__btn" data-role="prev" ${!canPrev ? 'disabled' : ''}>‹ Trước</button>
      <span class="pager__info">Trang ${page} / ${totalPages}</span>
      <button class="c-btn pager__btn" data-role="next" ${!canNext ? 'disabled' : ''}>Sau ›</button>
    </div>
  `;

  container.querySelector('[data-role="prev"]')?.addEventListener('click', () => {
    container.dispatchEvent(new CustomEvent('page-change', { detail: { page: page - 1 } }));
  });
  container.querySelector('[data-role="next"]')?.addEventListener('click', () => {
    container.dispatchEvent(new CustomEvent('page-change', { detail: { page: page + 1 } }));
  });
}

// ==== Page controller ====
export function initQuestionsPage() {
  // Route guard
  // if (!auth.isAuthenticated()) {
  //   window.location.href = '../auth/login.html';
  //   return;
  // }

  // Mount points
  const navHost = document.querySelector('[data-mount="navbar"]');
  const listHost = document.querySelector('[data-mount="list"]');
  const pagerHost = document.querySelector('[data-mount="pager"]');
  const searchInput = document.querySelector('[data-role="search"]');

  renderNavbar(navHost);

  // State
  let state = { page: 1, pageSize: 10, q: '' };

  async function load() {
    renderLoading(listHost);
    try {
      const res = await questionsApi.list(state);
      if (!res.items?.length) {
        renderEmpty(listHost);
      } else {
        renderList(listHost, res.items);
      }
      renderPagination(pagerHost, res);
    } catch (err) {
      console.error(err);
      renderError(listHost, err?.payload?.message || err.message);
      pagerHost.innerHTML = '';
    }
  }

  pagerHost.addEventListener('page-change', (e) => {
    state.page = e.detail.page;
    load();
  });

  // (Tuỳ chọn) Tìm kiếm đơn giản
searchInput?.addEventListener('keydown', (e) => {
  if (e.key === 'Enter') {
    const q = e.currentTarget.value.trim();
    const url = new URL('../search/search.html', location.href);
    if (q) url.searchParams.set('q', q);
    else url.searchParams.delete('q');
    // Giữ page=1 khi đổi truy vấn
    url.searchParams.set('page', '1');
    window.location.href = url.toString();
  }
});

  load();
}
