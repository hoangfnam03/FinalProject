// assets/js/api/stats.api.js
import { http } from '../core/http.js';

// GET /api/v1/stats/me -> UserStatsDto { questions, comments, votes, reputation }
export const statsApi = {
  getMyStats() {
    return http.get('/stats/me');
  },
};
