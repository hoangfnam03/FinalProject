// assets/js/entry/login.entry.js
import { applyTenantBranding } from "../core/env.js";
import { initLoginForm } from "../features/auth/login.js";

document.addEventListener("DOMContentLoaded", () => {
  applyTenantBranding(); // Đổi màu + tên theo TENANT
  initLoginForm(); // Gắn event submit form
});
