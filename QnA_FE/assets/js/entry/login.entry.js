// assets/js/entry/login.entry.js
import { initLayout } from '../core/layout.js';
import { initLoginPage } from '../features/auth/login.js';

document.addEventListener('DOMContentLoaded', async () => {
  initLoginPage();      // logic riêng trang login
});
