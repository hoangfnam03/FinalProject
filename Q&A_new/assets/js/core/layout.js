
import { logout } from "./auth.js";

export function applyPageClass(pageClass){
  if (!pageClass) return;
  document.body.classList.add(pageClass);
}

export function renderHeader() {
  const root = document.getElementById("appHeader");
  if (!root) return;
  root.innerHTML = `
    <div class="l-header">
      <div class="u-container u-between">
        <a href="../question/questions.html" class="text-decoration-none"><strong>IntraQ&A</strong></a>
        <nav class="d-flex align-items-center" style="gap:.5rem">
          <a class="btn btn-sm btn-outline-secondary" href="../question/questions.html">Câu hỏi</a>
          <a class="btn btn-sm btn-primary" href="../question/ask.html">Đặt câu hỏi</a>
          <button id="logoutBtn" class="btn btn-sm btn-outline-danger">Đăng xuất</button>
        </nav>
      </div>
    </div>
  `;
  const btn = root.querySelector("#logoutBtn");
  btn?.addEventListener("click", () => logout());
}
