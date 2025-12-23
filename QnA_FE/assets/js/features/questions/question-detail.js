// assets/js/features/questions/question-detail.js
import { questionApi } from '../../api/question.api.js';

function getQueryId() {
  const params = new URLSearchParams(window.location.search);
  return params.get('id');
}

export function initQuestionDetailPage() {
  const backBtn = document.querySelector('[data-action="back"]');
  if (backBtn) {
    backBtn.addEventListener('click', () => {
      history.length > 1
        ? window.history.back()
        : (window.location.href = '/page/question/questions.html');
    });
  }

  const id = getQueryId();
  if (!id) {
    const titleEl = document.querySelector('[data-role="question-title"]');
    if (titleEl) titleEl.textContent = 'Không tìm thấy câu hỏi.';
    return;
  }

  loadQuestion(id).catch(console.error);
}

async function loadQuestion(id) {
  const titleEl = document.querySelector('[data-role="question-title"]');
  const bodyEl = document.querySelector('[data-role="question-body"]');
  const tagsEl = document.querySelector('[data-role="question-tags"]');
  const votesEl = document.querySelector('[data-role="question-votes"]');
  const metaLeftEl = document.querySelector(
    '[data-role="question-meta-left"]'
  );
  const metaRightEl = document.querySelector(
    '[data-role="question-meta-right"]'
  );
  const answersListEl = document.querySelector('[data-role="answers-list"]');
  const answerCountEl = document.querySelector('[data-role="answer-count"]');

  if (titleEl) titleEl.textContent = 'Đang tải...';

  const detail = await questionApi.getById(id);

  const q = detail || {};

  // TITLE
  if (titleEl) {
    titleEl.textContent = q.title || 'Câu hỏi';
  }

  // VOTES
  const votes =
    q.votes ?? q.voteCount ?? q.score ?? 0;
  if (votesEl) votesEl.textContent = String(votes);

  // BODY (giản lược: xử lý HTML hoặc plain text)
  if (bodyEl) {
    const content = q.contentHtml || q.content || q.body || '';
    // nếu backend trả HTML an toàn có thể dùng innerHTML, tạm dùng:
    bodyEl.innerHTML = content;
  }

  // TAGS
  if (tagsEl) {
    const tags = q.tags || q.tagNames || [];
    tagsEl.innerHTML = tags
      .map((t) => `<span class="tag-pill">${escapeHtml(t)}</span>`)
      .join('');
  }

  // META
  const authorName =
    q.authorName ||
    q.author?.displayName ||
    q.author?.userName ||
    q.createdByName ||
    'Ẩn danh';

  const createdLabel =
    q.createdAtLabel || q.createdAgo || q.createdAt || '';

  if (metaLeftEl) {
    metaLeftEl.textContent = '';
  }

  if (metaRightEl) {
    metaRightEl.innerHTML = `
      <div class="question-card__author" style="justify-content:flex-end;">
        <div class="question-card__author-avatar"></div>
        <div>
          <div class="question-card__author-name">${escapeHtml(
            authorName
          )}</div>
          <div class="question-card__meta">${
            createdLabel ? escapeHtml(createdLabel) : ''
          }</div>
        </div>
      </div>
    `;
  }

  // ANSWERS
  const answers =
    q.answers || q.answerList || q.commentsAsAnswers || [];

  if (answerCountEl) {
    answerCountEl.textContent = String(answers.length);
  }

  if (answersListEl) {
    answersListEl.innerHTML = answers.length
      ? answers.map(renderAnswerCard).join('')
      : '<p class="text-muted">Chưa có câu trả lời nào.</p>';
  }
}

function renderAnswerCard(a) {
  const score = a.votes ?? a.voteCount ?? a.score ?? 0;
  const isAccepted = !!(a.isAccepted || a.accepted);

  const authorName =
    a.authorName ||
    a.author?.displayName ||
    a.author?.userName ||
    'Ẩn danh';
  const createdLabel =
    a.createdAtLabel || a.createdAgo || a.createdAt || '';

  const content = a.contentHtml || a.content || a.body || '';

  return `
    <article class="answer-card">
      <div class="answer-card__scorebar">
        <div class="answer-card__score">${score}</div>
        ${
          isAccepted
            ? '<div class="answer-card__accepted">✓</div>'
            : ''
        }
      </div>

      <div class="answer-card__body">
        <div class="answer-card__text">
          ${content}
        </div>
        <div class="answer-card__footer">
          <div>
            <a href="javascript:void(0)">Chia sẻ</a>
            ${
              a.commentCount
                ? ` · <span>Bình luận (${a.commentCount})</span>`
                : ''
            }
          </div>
          <div class="answer-card__author">
            <div class="answer-card__author-avatar"></div>
            <div>
              <div class="question-card__author-name">${escapeHtml(
                authorName
              )}</div>
              <div class="question-card__meta">${
                createdLabel ? escapeHtml(createdLabel) : ''
              }</div>
            </div>
          </div>
        </div>
      </div>
    </article>
  `;
}

function escapeHtml(str) {
  return (str || '')
    .toString()
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;');
}
