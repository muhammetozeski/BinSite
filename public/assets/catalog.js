'use strict';
const search = document.querySelector('#search');
const category = document.querySelector('#category');
const products = [...document.querySelectorAll('.product')];
function filter() {
  const query = search.value.trim().toLocaleLowerCase('en-US');
  for (const product of products) product.hidden = Boolean((query && !product.dataset.search.includes(query)) || (category.value && product.dataset.category !== category.value));
  const visible = products.filter(product => !product.hidden);
  document.querySelector('#results').textContent = query || category.value ? `${visible.length} matches` : '';
  document.querySelector('#empty').hidden = visible.length > 0;
  for (const section of document.querySelectorAll('[data-product-group]')) section.hidden = ![...section.querySelectorAll('.product')].some(product => !product.hidden);
}
search.addEventListener('input', filter);
category.addEventListener('change', filter);
