// assets/js/entry/questions.entry.js
import { applyTenantBranding } from "../core/env.js";
import { initLayout } from "../core/layout.js";
import { initQuestionsPage } from "../features/questions/questions.js";

document.addEventListener("DOMContentLoaded", async () => {
  applyTenantBranding(); // đa tổ chức: đổi màu + tên thương hiệu
  await initLayout({ activeNav: "home" });
  initQuestionsPage();
});
