// assets/js/features/questions/post-create.js
import { auth } from '../../core/auth.js';
import { questionsApi } from '../../api/questions.api.js';
import { categoriesApi } from '../../api/categories.api.js';

function setLoading(btn, loading) {
  if (!btn) return;
  btn.disabled = loading;
  btn.dataset.loading = loading ? '1' : '0';
  btn.textContent = loading ? 'Đang đăng…' : 'Đăng câu hỏi';
}

function showAlert(el, msg, type = 'danger') {
  if (!el) return;
  el.classList.add('is-visible');
  el.classList.remove('c-alert--success');
  if (type === 'success') el.classList.add('c-alert--success');
  el.textContent = msg;
}

function parseTags(raw) {
  return (raw || '')
    .split(',')
    .map(s => s.trim())
    .filter(Boolean)
    .slice(0, 10); // hạn chế tối đa 10 tag
}

export async function initPostCreatePage() {
  // Guard
  if (!auth.isAuthenticated()) {
    window.location.href = '../auth/login.html';
    return;
  }

  // Mounts
  const form = document.querySelector('#askForm');
  const titleEl = form?.querySelector('[name="title"]');
  const bodyEl = form?.querySelector('[name="body"]');
  const tagsEl = form?.querySelector('[name="tags"]');
  const catSelect = form?.querySelector('[name="categoryId"]');
  const submitBtn = form?.querySelector('[data-role="submit"]');
  const alertEl = form?.querySelector('[data-role="alert"]');

  // Nạp danh mục
  try {
    const cats = await categoriesApi.list();
    if (Array.isArray(cats) && catSelect) {
      catSelect.innerHTML = `<option value="">-- Chọn danh mục --</option>` +
        cats.map(c => `<option value="${c.id}" data-slug="${c.slug}">${c.name}</option>`).join('');
    }
  } catch (e) {
    console.warn('Không nạp được danh mục:', e);
  }

  // Submit
  form?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const title = titleEl?.value?.trim();
    const body = bodyEl?.value?.trim();
    const tags = parseTags(tagsEl?.value);
    const selectedOpt = catSelect?.selectedOptions?.[0];
    const categoryId = selectedOpt?.value ? Number(selectedOpt.value) : null;
    const categorySlug = selectedOpt?.dataset?.slug || null;

    if (!title || !body) {
      showAlert(alertEl, 'Vui lòng nhập tiêu đề và nội dung.');
      return;
    }

    try {
      setLoading(submitBtn, true);
      const created = await questionsApi.create({
        title,
        body,
        tags,
        categoryId,
        categorySlug,
      });
      // Trả về PostDetailDto → có id
      window.location.href = `./question-detail.html?id=${created.id}`;
    } catch (err) {
      console.error(err);
      const msg = err?.payload?.message || err.message || 'Đăng câu hỏi thất bại.';
      showAlert(alertEl, msg);
    } finally {
      setLoading(submitBtn, false);
    }
  });
}
