// assets/js/core/layout.js
import { auth } from './auth.js';
import { qs, qsa } from './dom.js';
import { notificationApi } from '../api/notification.api.js';


async function loadPartial(selector, url) {
  const container = qs(selector);
  if (!container) return;
  const res = await fetch(url);
  container.innerHTML = await res.text();
}

async function loadPartials() {
  await Promise.all([
    loadPartial('[data-partial="header"]', '/partials/header.html'),
    loadPartial('[data-partial="footer"]', '/partials/footer.html'),
  ]);
}

function setActiveNav() {
  const path = location.pathname;
  const current = path.split('/').pop(); // ví dụ questions.html

  qsa('.main-nav a').forEach((a) => {
    const href = a.getAttribute('href') || '';
    const last = href.split('/').pop();
    if (last === current) {
      a.classList.add('active');
    } else {
      a.classList.remove('active');
    }
  });
}

function initHeaderSearch() {
  const btn = qs('[data-action="toggle-search"]');
  const form = qs('[data-role="header-search"]');

  if (!btn || !form) return;

  btn.addEventListener('click', () => {
    form.classList.toggle('is-open');
    const input = form.querySelector('input[name="q"]');
    if (form.classList.contains('is-open') && input) input.focus();
  });

  form.addEventListener('submit', (e) => {
    e.preventDefault();
    const formData = new FormData(form);
    const q = (formData.get('q') || '').toString().trim();
    if (!q) return;

    // TRƯỚC: /search.html?q=...
    // SAU: thêm type=posts
    location.href = `/page/search/search.html?q=${encodeURIComponent(
      q
    )}&type=posts`;
});
}

function formatNumberForRep(value) {
  const num = Number(value) || 0;
  if (num >= 1000) {
    return num.toLocaleString('en-US'); // 1240 -> 1,240
  }
  return String(num);
}

function initUserMenu() {
  const userMenu = qs('[data-role="user-menu"]');
  if (!userMenu) return;

  const me = auth.getCurrentUser();

  // CHƯA ĐĂNG NHẬP -> nút Đăng nhập
  if (!me) {
    userMenu.innerHTML = `
      <button class="btn btn-ghost" data-action="login-popup" type="button">
        Đăng nhập
      </button>
    `;

    const loginBtn = userMenu.querySelector('[data-action="login-popup"]');
    if (loginBtn) {
      loginBtn.addEventListener('click', () => {
        window.location.href = '/page/auth/login.html';
      });
    }
    return;
  }

  // ĐÃ ĐĂNG NHẬP -> user chip (avatar + tên + rep)
  const name =
    me.displayName ||
    me.fullName ||
    me.userName ||
    me.email ||
    'User';

  const rep =
    me.reputation ??
    me.reputationScore ??
    me.rep ??
    0;

  const avatarUrl = me.avatarUrl || me.avatar || '';

  userMenu.innerHTML = `
    <div class="user-chip" data-action="go-profile">
      <div class="user-chip__avatar">
        ${
          avatarUrl
            ? `<img src="${avatarUrl}" alt="${name}" />`
            : `<span class="user-chip__avatar-fallback">${
                (name[0] || 'U').toUpperCase()
              }</span>`
        }
      </div>
      <div class="user-chip__info">
        <div class="user-chip__name">${name}</div>
        <div class="user-chip__rep">
          Rep: ${formatNumberForRep(rep)}
          ·
          <button
            type="button"
            class="user-chip__logout"
            data-action="logout"
          >
            Đăng xuất
          </button>
        </div>
      </div>
    </div>
  `;

  const profileChip = userMenu.querySelector('[data-action="go-profile"]');
  if (profileChip) {
    profileChip.addEventListener('click', () => {
      window.location.href = '/page/user/profile.html';
    });
  }

  const logoutBtn = userMenu.querySelector('[data-action="logout"]');
  if (logoutBtn) {
    logoutBtn.addEventListener('click', (e) => {
      e.stopPropagation(); // không trigger go-profile
      auth.clearAuth();
      location.href = '/page/auth/login.html';
    });
  }
}


function initNotificationWidget() {
  const wrapper = document.querySelector('[data-role="notification"]');
  if (!wrapper) return;

  const toggleBtn = wrapper.querySelector(
    '[data-action="toggle-notifications"]'
  );
  const dropdown = wrapper.querySelector(
    '[data-role="notification-dropdown"]'
  );
  const itemsContainer = wrapper.querySelector(
    '[data-role="notification-items"]'
  );
  const badge = wrapper.querySelector('[data-role="notification-badge"]');
  const viewAllBtn = wrapper.querySelector(
    '[data-action="view-all-notifications"]'
  );
  const markAllBtn = wrapper.querySelector(
    '[data-action="mark-all-read"]'
  );

  if (!toggleBtn || !dropdown || !itemsContainer) return;

  toggleBtn.addEventListener('click', async (e) => {
    e.stopPropagation();
    const isOpen = dropdown.classList.contains('is-open');
    document
      .querySelectorAll('.notification-dropdown.is-open')
      .forEach((el) => el.classList.remove('is-open'));

    if (!isOpen) {
      dropdown.classList.add('is-open');
      await loadNotificationsPreview(itemsContainer, badge);
    }
  });

  // click ngoài dropdown thì đóng
  document.addEventListener('click', () => {
    dropdown.classList.remove('is-open');
  });
  dropdown.addEventListener('click', (e) => e.stopPropagation());

  // View all -> sang trang notifications
  if (viewAllBtn) {
    viewAllBtn.addEventListener('click', () => {
      window.location.href = '/page/notification/notifications.html';
    });
  }

  if (markAllBtn) {
    markAllBtn.addEventListener('click', async (e) => {
      e.stopPropagation();
      await markAllCurrentAsRead(itemsContainer, badge);
    });
  }

  // load badge lần đầu
  loadUnreadCount(badge).catch(console.error);
}

async function loadUnreadCount(badgeEl) {
  if (!badgeEl) return;
  try {
    const count = await notificationApi.getUnreadCount();
    if (!count || count <= 0) {
      badgeEl.classList.remove('is-visible');
      badgeEl.textContent = '';
    } else {
      badgeEl.textContent = count > 9 ? '9+' : String(count);
      badgeEl.classList.add('is-visible');
    }
  } catch (e) {
    console.error(e);
  }
}

async function loadNotificationsPreview(itemsContainer, badgeEl) {
  try {
    itemsContainer.innerHTML = '<p class="text-muted">Loading...</p>';
    const paged = await notificationApi.getList({ page: 1, pageSize: 5 });

    const items = paged.items || [];
    if (!items.length) {
      itemsContainer.innerHTML = '<p class="text-muted">No notifications.</p>';
      badgeEl?.classList.remove('is-visible');
      badgeEl.textContent = '';
      return;
    }

    itemsContainer.innerHTML = items
      .map((n) => {
        const iconType =
          (n.type && n.type.toLowerCase().includes('accept')) ||
          n.category === 'AnswerAccepted'
            ? 'success'
            : 'info';

        return `
          <div class="notification-item ${
            n.isRead ? '' : 'notification-item--unread'
          }"
               data-id="${n.id}"
               data-action-url="${n.actionUrl || ''}">
            <div class="notification-item__icon notification-item__icon--${iconType}">
              ${iconType === 'success' ? '✓' : '↑'}
            </div>
            <div class="notification-item__content">
              <div class="notification-item__title">
                ${n.title || n.postTitle || 'Thông báo'}
              </div>
              ${
                n.excerpt
                  ? `<div class="notification-item__excerpt">${n.excerpt}</div>`
                  : ''
              }
              <span class="notification-item__time">
                ${n.createdAt || n.createdAtLabel || ''}
              </span>
            </div>
          </div>
        `;
      })
      .join('');


    // click 1 item -> mark read + chuyển trang (nếu có actionUrl)
    itemsContainer
      .querySelectorAll('.notification-item')
      .forEach((itemEl) => {
        itemEl.addEventListener('click', async () => {
          const id = Number(itemEl.dataset.id);
          const url = itemEl.dataset.actionUrl;

          try {
            await notificationApi.markRead([id]);
            itemEl.classList.remove('notification-item--unread');
            // cập nhật badge
            await loadUnreadCount(badgeEl);
          } catch (e) {
            console.error(e);
          }

          if (url) {
            window.location.href = url;
          }
        });
      });
  } catch (e) {
    console.error(e);
    itemsContainer.innerHTML =
      '<p class="text-muted">Không tải được notifications.</p>';
  }
}

async function markAllCurrentAsRead(itemsContainer, badgeEl) {
  if (!itemsContainer) return;
  const ids = [];
  itemsContainer.querySelectorAll('.notification-item').forEach((el) => {
    const id = Number(el.dataset.id);
    if (id) ids.push(id);
  });
  if (!ids.length) return;

  try {
    await notificationApi.markRead(ids);
    // cập nhật UI
    itemsContainer
      .querySelectorAll('.notification-item')
      .forEach((el) => el.classList.remove('notification-item--unread'));
    await loadUnreadCount(badgeEl);
  } catch (e) {
    console.error(e);
  }
}


function initAskQuestionButton() {
  const btn = qs('[data-action="ask-question"]');
  if (!btn) return;
  btn.addEventListener('click', () => {
    window.location.href = '/page/question/question-create.html';
  });
}

export async function initLayout() {
  await loadPartials(); // inject header/footer
  setActiveNav();       // tô màu nav hiện tại
  initHeaderSearch();   // behavior search chung
  initUserMenu();       // hiển thị user / login
  initNotificationWidget(); // hiển thị notifications
  initAskQuestionButton(); // nút hỏi question
}
