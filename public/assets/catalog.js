'use strict';
const search = document.querySelector('#search');
const products = [...document.querySelectorAll('.product')];
function filter() {
  const terms = search.value.trim().toLocaleLowerCase('en-US').split(/\s+/).filter(Boolean);
  for (const product of products) product.hidden = !terms.every(term => product.dataset.search.includes(term));
  const visible = products.filter(product => !product.hidden);
  document.querySelector('#results').textContent = terms.length && visible.length ? `${visible.length} ${visible.length === 1 ? 'match' : 'matches'}` : '';
  document.querySelector('#empty').hidden = visible.length > 0;
  for (const section of document.querySelectorAll('[data-product-group]')) {
    section.hidden = ![...section.querySelectorAll('.product')].some(product => !product.hidden);
    document.querySelector(`nav a[href="#${section.id}"]`).hidden = section.hidden;
  }
}
search.addEventListener('input', filter);
