// assets/js/api/question.api.js
import { httpClient } from "../core/http.js";

export const questionApi = {
  /**
   * Lấy danh sách câu hỏi
   * BE tuỳ chỉnh lại param nếu bạn dùng khác.
   */
  async getList({ page = 1, pageSize = 10, sort = "latest" } = {}) {
    const params = new URLSearchParams({
      page: String(page),
      pageSize: String(pageSize),
      sort,
    });

    return httpClient.get(`/api/questions?${params.toString()}`);
  },
};
