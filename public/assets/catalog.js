'use strict';
const search = document.querySelector('#search');
const category = document.querySelector('#category');
const projects = [...document.querySelectorAll('#projects article')];
function filter() {
  const query = search.value.trim().toLocaleLowerCase('tr-TR');
  for (const project of projects) project.hidden = Boolean((query && !project.dataset.search.includes(query)) || (category.value && project.dataset.category !== category.value));
  const visible = projects.filter(project => !project.hidden);
  document.querySelector('#results').textContent = `${visible.length} proje gösteriliyor.`;
  document.querySelector('#empty').hidden = visible.length > 0;
  document.querySelector('#surprise').disabled = visible.length === 0;
}
search.addEventListener('input', filter);
category.addEventListener('change', filter);
document.querySelector('#surprise').addEventListener('click', () => {
  const visible = projects.filter(project => !project.hidden);
  if (visible.length) location.href = visible[Math.floor(Math.random() * visible.length)].querySelector('h2 a').href;
});
