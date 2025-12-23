// assets/js/core/http.js
import { env } from "./env.js";

async function request(path, options = {}) {
  const url =
    (env.BASE_API_URL?.replace(/\/+$/, "") || "") +
    "/" +
    path.replace(/^\/+/, "");

  const defaultHeaders = {
    "Content-Type": "application/json",
  };

  const res = await fetch(url, {
    ...options,
    headers: {
      ...defaultHeaders,
      ...(options.headers || {}),
    },
  });

  let data = null;
  const text = await res.text();
  if (text) {
    try {
      data = JSON.parse(text);
    } catch {
      data = text;
    }
  }

  if (!res.ok) {
    const message =
      data?.message || data?.error || `Request failed with status ${res.status}`;
    throw new Error(message);
  }

  return data;
}

export const httpClient = {
  get: (path) => request(path),
  post: (path, body) =>
    request(path, {
      method: "POST",
      body: JSON.stringify(body),
    }),
};
