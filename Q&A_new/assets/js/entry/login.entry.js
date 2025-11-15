// assets/js/entry/login.entry.js
import { setupLoginForm } from '../features/auth/login.js';

document.addEventListener('DOMContentLoaded', () => {
  setupLoginForm({
    formSelector: '#loginForm',
    // Sau khi login sẽ về trang câu hỏi
    redirectTo: '/page/question/questions.html',
  });
});
