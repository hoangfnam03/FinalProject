// assets/js/features/questions/questions.js
import { questionApi } from '../../api/question.api.js';

let state = {
  page: 1,
  pageSize: 10,
  sort: 'newest',
};

export function initQuestionsPage() {
  const sortContainer = document.querySelector('[data-role="question-sort"]');

  if (sortContainer) {
    sortContainer.addEventListener('click', (e) => {
      const btn = e.target.closest('[data-sort]');
      if (!btn) return;

      const sort = btn.dataset.sort;
      if (!sort || sort === state.sort) return;

      state.sort = sort;
      state.page = 1;

      // toggle active
      sortContainer
        .querySelectorAll('.btn-tab')
        .forEach((b) => b.classList.remove('is-active'));
      btn.classList.add('is-active');

      loadQuestions();
    });
  }

  loadQuestions();
}

async function loadQuestions() {
  const listEl = document.querySelector('[data-role="question-list"]');
  const paginationEl = document.querySelector('[data-role="pagination"]');

  if (!listEl) return;

  listEl.innerHTML = '<p class="text-muted">Đang tải câu hỏi...</p>';
  if (paginationEl) paginationEl.innerHTML = '';

  try {
    const paged = await questionApi.getList({
      page: state.page,
      pageSize: state.pageSize,
      sort: state.sort,
    });

    const items = paged.items || paged.data || [];
    listEl.innerHTML = items.length
      ? items.map(renderQuestionCard).join('')
      : '<p class="text-muted">Chưa có câu hỏi nào.</p>';

    if (paginationEl) {
      renderPagination(paginationEl, {
        page: paged.page || state.page,
        pageSize: paged.pageSize || state.pageSize,
        totalItems: paged.totalItems ?? paged.total ?? 0,
        totalPages: paged.totalPages ?? Math.ceil((paged.totalItems ?? 0) / state.pageSize),
      });
    }
  } catch (err) {
    console.error(err);
    listEl.innerHTML =
      '<p class="text-muted">Không tải được danh sách câu hỏi.</p>';
  }
}

function renderQuestionCard(q) {
  const url = `/page/question/question-detail.html?id=${encodeURIComponent(
    q.id
  )}`;

  const tags = (q.tags || q.tagNames || []).map(
    (t) => `<span class="tag-pill">${escapeHtml(t)}</span>`
  );

  const votes = q.votes ?? q.voteCount ?? 0;
  const ageLabel = q.ageLabel || q.createdAgo || '';

  const authorName = q.authorName || q.author?.displayName || 'Ẩn danh';
  const authorAvatar = q.authorAvatarUrl || q.author?.avatarUrl || '';

  const createdAtLabel = q.createdAtLabel || q.createdAgo || '';

  return `
    <article class="question-card">
      <div class="question-card__stats">
        <div class="question-card__stats-label">Votes</div>
        <div class="question-card__stats-value">${votes}</div>
        ${
          ageLabel
            ? `<div class="question-card__age">${escapeHtml(ageLabel)}</div>`
            : ''
        }
      </div>

      <div class="question-card__content">
        <h2 class="question-card__title">
          <a href="${url}">${escapeHtml(q.title || '')}</a>
        </h2>

        ${
          q.excerpt || q.summary
            ? `<p class="question-card__excerpt">${escapeHtml(
                q.excerpt || q.summary
              )}</p>`
            : ''
        }

        ${
          tags.length
            ? `<div class="question-card__tags">${tags.join('')}</div>`
            : ''
        }

        <div class="question-card__footer">
          <div class="question-card__author">
            <div class="question-card__author-avatar">
              ${
                authorAvatar
                  ? `<img src="${authorAvatar}" alt="${escapeHtml(
                      authorName
                    )}" />`
                  : ''
              }
            </div>
            <div>
              <div class="question-card__author-name">${escapeHtml(
                authorName
              )}</div>
              <div class="question-card__meta">
                ${createdAtLabel ? escapeHtml(createdAtLabel) : ''}
              </div>
            </div>
          </div>
        </div>
      </div>
    </article>
  `;
}

/** Pagination renderer */
function renderPagination(container, { page, totalPages }) {
  if (!totalPages || totalPages <= 1) {
    container.innerHTML = '';
    return;
  }

  let html = '';

  // Prev
  html += `<button class="pagination__btn" data-page="${
    page - 1
  }" ${page <= 1 ? 'disabled' : ''}>&laquo;</button>`;

  // Simple window: current -1, current, current +1
  const start = Math.max(1, page - 1);
  const end = Math.min(totalPages, page + 1);

  if (start > 1) {
    html += `<button class="pagination__btn" data-page="1">1</button>`;
    if (start > 2) {
      html += `<span class="pagination__ellipsis">...</span>`;
    }
  }

  for (let p = start; p <= end; p++) {
    html += `<button class="pagination__btn ${
      p === page ? 'is-active' : ''
    }" data-page="${p}">${p}</button>`;
  }

  if (end < totalPages) {
    if (end < totalPages - 1) {
      html += `<span class="pagination__ellipsis">...</span>`;
    }
    html += `<button class="pagination__btn" data-page="${totalPages}">${totalPages}</button>`;
  }

  // Next
  html += `<button class="pagination__btn" data-page="${
    page + 1
  }" ${page >= totalPages ? 'disabled' : ''}>&raquo;</button>`;

  container.innerHTML = html;

  container.querySelectorAll('.pagination__btn[data-page]').forEach((btn) => {
    btn.addEventListener('click', () => {
      const targetPage = Number(btn.dataset.page);
      if (!targetPage || targetPage === state.page) return;
      state.page = targetPage;
      loadQuestions();
    });
  });
}

function escapeHtml(str) {
  return (str || '')
    .toString()
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;');
}
