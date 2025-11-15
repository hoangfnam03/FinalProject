import { initPostCreatePage } from '../features/questions/post-create.js';
import { initBell } from '../features/notifications/bell.js';

document.addEventListener('DOMContentLoaded', () => {
  initPostCreatePage();
  const host = document.querySelector('[data-mount="notif"]');
  initBell(host);
});
