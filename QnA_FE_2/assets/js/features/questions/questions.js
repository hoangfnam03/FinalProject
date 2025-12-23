// assets/js/features/questions/questions.js
import { questionApi } from "../../api/question.api.js";

const state = {
  page: 1,
  pageSize: 10,
  totalPages: 1,
  sort: "latest",
};

let listContainer;
let paginationContainer;
let tabButtons;

/**
 * Khởi tạo trang câu hỏi
 */
export function initQuestionsPage() {
  listContainer = document.querySelector('[data-role="question-list"]');
  paginationContainer = document.querySelector('[data-role="pagination"]');
  tabButtons = document.querySelectorAll('[data-role="question-tab"]');

  if (!listContainer) return;

  // tabs
  tabButtons.forEach((btn) => {
    btn.addEventListener("click", () => {
      const sort = btn.getAttribute("data-sort") || "latest";
      state.sort = sort;
      state.page = 1;

      tabButtons.forEach((b) => b.classList.remove("is-active"));
      btn.classList.add("is-active");

      loadQuestions();
    });
  });

  loadQuestions();
}

/**
 * gọi API, nếu lỗi thì hiển thị data demo
 */
async function loadQuestions() {
  // Giữ nguyên danh sách cũ, chỉ báo loading ở phần phân trang
  if (paginationContainer) {
    paginationContainer.innerHTML =
      '<span class="text-soft">Đang tải dữ liệu...</span>';
  }

  try {
    const res = await questionApi.getList({
      page: state.page,
      pageSize: state.pageSize,
      sort: state.sort,
    });

    const items = res.items || res.data || [];
    state.page = res.pageNumber || res.page || state.page;
    state.totalPages = res.totalPages || res.totalPage || 1;

    renderQuestionList(items);
    renderPagination();
  } catch (err) {
    console.error("Lỗi load câu hỏi, dùng data demo:", err);
    const demoItems = getDemoQuestions();
    state.page = 1;
    state.totalPages = 1;
    renderQuestionList(demoItems);
    renderPagination(); // để xoá chữ "Đang tải..."
  }
}


/**
 * render list
 */
function renderQuestionList(items) {
  if (!items || items.length === 0) {
    listContainer.innerHTML =
      '<div class="text-soft">Chưa có câu hỏi nào.</div>';
    return;
  }

  const html = items
    .map((q) => {
      const tags = (q.tags || []).map(
        (t) => `<span class="question-tag">${t}</span>`
      );

      return `
      <article class="question-card">
        <div class="question-meta">
          <div class="question-meta-label">Votes</div>
          <div class="question-meta-value">${q.votes ?? 0}</div>
          ${
            q.answersCount != null
              ? `<div class="question-meta-pill">${q.answersCount} ans</div>`
              : ""
          }
        </div>

        <div class="question-main">
          <a href="${
            q.detailUrl || "/page/question/question-detail.html"
          }" class="question-title">
            ${escapeHtml(q.title || "")}
          </a>
          <p class="question-excerpt">
            ${escapeHtml(q.excerpt || "")}
          </p>

          <div class="question-tags">
            ${tags.join("")}
          </div>

          <div class="question-footer">
            <div class="question-author">
              <div class="question-author-avatar">
                <img src="${
                  q.authorAvatar ||
                  "https://i.pravatar.cc/40?img=31"
                }" alt="">
              </div>
              <span class="question-author-name">${
                q.authorName || "Ẩn danh"
              }</span>
              <span>•</span>
              <span>${q.createdAgo || "vừa xong"}</span>
            </div>
          </div>
        </div>
      </article>
    `;
    })
    .join("");

  listContainer.innerHTML = html;
}

/**
 * render phân trang đơn giản: Prev [1] [2] ... Next
 */
function renderPagination() {
  if (!paginationContainer) return;
  if (state.totalPages <= 1) {
    paginationContainer.innerHTML = "";
    return;
  }

  const buttons = [];

  // Prev
  buttons.push(`
    <button class="pagination-btn" ${
      state.page <= 1 ? "disabled" : ""
    } data-page="${state.page - 1}">
      ‹
    </button>
  `);

  // số trang (tối đa 5 nút cho gọn)
  const maxButtons = 5;
  let start = Math.max(1, state.page - 2);
  let end = Math.min(state.totalPages, start + maxButtons - 1);
  if (end - start + 1 < maxButtons) {
    start = Math.max(1, end - maxButtons + 1);
  }

  for (let p = start; p <= end; p++) {
    buttons.push(
      `<button class="pagination-btn ${
        p === state.page ? "is-active" : ""
      }" data-page="${p}">${p}</button>`
    );
  }

  // Next
  buttons.push(`
    <button class="pagination-btn" ${
      state.page >= state.totalPages ? "disabled" : ""
    } data-page="${state.page + 1}">
      ›
    </button>
  `);

  paginationContainer.innerHTML = buttons.join("");

  // gắn event
  paginationContainer
    .querySelectorAll(".pagination-btn")
    .forEach((btn) => {
      const pageAttr = btn.getAttribute("data-page");
      if (!pageAttr) return;

      btn.addEventListener("click", () => {
        const page = Number(pageAttr);
        if (!page || page === state.page || page < 1) return;
        state.page = page;
        loadQuestions();
      });
    });
}

/**
 * Data demo giống trong hình, dùng khi chưa nối BE
 */
function getDemoQuestions() {
  return [
    {
      id: 1,
      title:
        'Làm thế nào để fix lỗi "useEffect infinite loop" khi fetch data?',
      excerpt:
        "Tôi đang có gọi API trong useEffect nhưng nó cứ chạy liên tục không dừng. Tôi đã thêm dependency array rỗng nhưng vẫn bị...",
      votes: 142,
      answersCount: 12,
      tags: ["reactjs", "javascript", "hooks"],
      authorName: "Nam Nguyen",
      createdAgo: "2 giờ trước",
      authorAvatar: "https://i.pravatar.cc/40?img=15",
    },
    {
      id: 2,
      title: "Sự khác biệt giữa Docker Image và Container là gì?",
      excerpt:
        "Tôi mới học DevOps và cảm thấy hơi bối rối về khái niệm này. Có phải Image giống như class còn Container giống như object trong OOP không?",
      votes: 45,
      answersCount: 3,
      tags: ["docker", "devops"],
      authorName: "Mai Anh",
      createdAgo: "5 giờ trước",
      authorAvatar: "https://i.pravatar.cc/40?img=32",
    },
  ];
}

function escapeHtml(str) {
  return String(str)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;");
}
