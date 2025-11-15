// assets/js/api/questions.api.js
import { http } from '../core/http.js';

function buildQuery({ page = 1, pageSize = 10, q = '' } = {}) {
  const params = new URLSearchParams();
  params.set('page', page);
  params.set('pageSize', pageSize);
  if (q) params.set('q', q);
  return `?${params.toString()}`;
}

function mapPost(p) {
  return {
    id: p.id,
    title: p.title,
    authorName: p.authorDisplayName,
    createdAt: p.createdAt,
    votes: p.score,                 // map score -> votes hiển thị
    tags: p.tags ?? [],
    preview: p.previewBody ?? '',
  };
}

/**
 * BE: 
 *  - GET /api/v1/posts → { page, pageSize, total, items: PostDto[] }
 *  - POST /api/v1/posts (CreatePostRequest)
 */
export const questionsApi = {
  async list({ page = 1, pageSize = 10, q = '' } = {}) {
    const query = buildQuery({ page, pageSize, q });
    const res = await http.get(`/posts${query}`);

    // mock 
    // const response = await fetch('/mockdata/Post/PostDtoPaged.json');
    // if (!response.ok) throw new Error('Failed to fetch mock data');
    // const res = await response.json(); 


    return {
      items: (res.items ?? []).map(mapPost),
      total: Number(res.total ?? 0),
      page: Number(res.page ?? page),
      pageSize: Number(res.pageSize ?? pageSize),
    };
  },

  async create({ title, body, tags, categoryId, categorySlug }) {
    // { title, body, tags: string[], categorySlug, categoryId? }
    return http.post('/posts', {
      title,
      body,
      tags,
      categorySlug,
      categoryId,
    });
  },
};
