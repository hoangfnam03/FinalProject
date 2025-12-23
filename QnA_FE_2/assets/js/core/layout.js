// assets/js/core/layout.js

/**
 * Load partials (header/footer) vào các div có data-partial="header/footer"
 */
export async function loadPartials() {
  const containers = document.querySelectorAll("[data-partial]");
  const tasks = [];

  containers.forEach((el) => {
    const name = el.getAttribute("data-partial");
    if (!name) return;

    const url = `/partials/${name}.html`;
    const task = fetch(url)
      .then((res) => res.text())
      .then((html) => {
        el.innerHTML = html;
      })
      .catch((err) => {
        console.error(`Không load được partial: ${name}`, err);
      });

    tasks.push(task);
  });

  await Promise.all(tasks);
}

/**
 * Highlight menu bên sidebar
 */
export function setActiveNav(key) {
  if (!key) return;
  const items = document.querySelectorAll("[data-nav-key]");
  items.forEach((el) => {
    const navKey = el.getAttribute("data-nav-key");
    if (navKey === key) {
      el.classList.add("is-active");
    } else {
      el.classList.remove("is-active");
    }
  });
}

/**
 * (optional) init search
 */
export function initHeaderSearch() {
  const input = document.querySelector('[data-role="global-search-input"]');
  if (!input) return;

  input.addEventListener("keydown", (e) => {
    if (e.key === "Enter") {
      const q = input.value.trim();
      if (!q) return;
      // TODO: chuyển sang trang search
      console.log("Search:", q);
      // window.location.href = `/page/search/search.html?q=${encodeURIComponent(q)}`;
    }
  });
}

/**
 * Hàm tiện dùng chung cho mọi trang có header/footer
 */
export async function initLayout(options = {}) {
  await loadPartials();
  setActiveNav(options.activeNav || null);
  initHeaderSearch();
}
