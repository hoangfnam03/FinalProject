// assets/js/entry/questions.entry.js
import { initLayout } from '../core/layout.js';
import { initQuestionsPage } from '../features/questions/questions.js';

document.addEventListener('DOMContentLoaded', async () => {
  await initLayout();      // header/footer + menu, search, user...
  initQuestionsPage();     // logic riêng trang danh sách câu hỏi
});
