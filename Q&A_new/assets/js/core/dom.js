
export const $ = (sel, root=document) => root.querySelector(sel);
export const $$ = (sel, root=document) => Array.from(root.querySelectorAll(sel));

export function formToJSON(form){
  const data = {};
  new FormData(form).forEach((v,k) => { data[k] = typeof v === "string" ? v.trim() : v; });
  return data;
}

export function setText(el, text, className){
  if (!el) return;
  el.textContent = text || "";
  el.className = className || "";
}
