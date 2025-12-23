// assets/js/features/auth/login.js
import { authApi } from "../../api/auth.api.js";
import { handleLoginSuccess } from "../../core/auth.js";

export function initLoginForm() {
  const form = document.getElementById("login-form");
  if (!form) return;

  const emailInput = document.getElementById("email");
  const passwordInput = document.getElementById("password");
  const submitBtn = document.getElementById("login-submit-btn");
  const errorBox = document.getElementById("login-error");

  function showError(message) {
    if (!errorBox) return;
    errorBox.textContent = message;
    errorBox.hidden = !message;
  }

  async function handleSubmit(e) {
    e.preventDefault();
    showError("");

    const email = emailInput.value.trim();
    const password = passwordInput.value;

    if (!email || !password) {
      showError("Vui lòng nhập đầy đủ email và mật khẩu.");
      return;
    }

    submitBtn.disabled = true;

    try {
      const result = await authApi.login(email, password);
      handleLoginSuccess(result);

      // TODO: Redirect sau khi login thành công
      // Ví dụ quay về trang danh sách câu hỏi
      window.location.href = "/page/question/questions.html";
    } catch (err) {
      console.error("Login failed:", err);
      showError(err.message || "Email hoặc mật khẩu không đúng (hoặc server lỗi).");
    } finally {
      submitBtn.disabled = false;
    }
  }

  form.addEventListener("submit", handleSubmit);
}
