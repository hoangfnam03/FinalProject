// assets/js/core/env.js

// Cấu hình theo từng tổ chức.
// Chỉ cần copy file này và đổi các giá trị cho từng tenant.

export const env = {
  BASE_API_URL: "http://localhost:7006", // URL BE của bạn

  TENANT: {
    key: "devask",
    brandName: "DevAsk",
    tagline:
      "Cộng đồng chia sẻ kiến thức, gỡ lỗi và phát triển sự nghiệp dành cho lập trình viên.",
    primaryColor: "#2563eb",
    primarySoft: "#1d4ed8",
  },
};

/**
 * Áp dụng màu + text từ TENANT ra giao diện
 */
export function applyTenantBranding() {
  const { TENANT } = env;

  // Đổi màu brand bằng CSS variables
  if (TENANT.primaryColor) {
    document.documentElement.style.setProperty(
      "--brand-primary",
      TENANT.primaryColor
    );
  }
  if (TENANT.primarySoft) {
    document.documentElement.style.setProperty(
      "--brand-primary-soft",
      TENANT.primarySoft
    );
  }

  // Đổi text brand name + tagline
  const brandNameEls = document.querySelectorAll('[data-role="brand-name"]');
  brandNameEls.forEach((el) => {
    el.textContent = TENANT.brandName ?? "DevAsk";
  });

  const taglineEls = document.querySelectorAll('[data-role="brand-tagline"]');
  taglineEls.forEach((el) => {
    el.textContent =
      TENANT.tagline ??
      "Cộng đồng chia sẻ kiến thức, gỡ lỗi và phát triển sự nghiệp dành cho lập trình viên.";
  });
}
