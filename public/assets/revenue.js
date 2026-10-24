'use strict';
(async function () {
  const adEligible = document.currentScript?.dataset.ads !== 'off';
  const footer = document.createElement('aside');
  footer.setAttribute('aria-label', 'Site links');
  footer.style.cssText = 'clear:both;max-width:1100px;margin:32px auto 12px;padding:16px;border-top:1px solid #aaa6;font:14px/1.6 system-ui,sans-serif;position:static';
  const currentPath = location.pathname.replace(/\/+$/, '') || '/';
  const links = [['Account', '/uyelik/'], ['Privacy', '/privacy/'], ['Home', '/']].filter(([, href]) => (href.replace(/\/+$/, '') || '/') !== currentPath);
  const navigation = document.createElement('span');
  navigation.setAttribute('role', 'navigation');
  navigation.setAttribute('aria-label', 'Site');
  footer.append(navigation);
  document.body.append(footer);
  let renderedLinks;
  function insideClosedContent(anchor) {
    for (let parent = anchor.parentElement; parent; parent = parent.parentElement) {
      if (parent.tagName !== 'DETAILS' || parent.open) continue;
      const summary = [...parent.children].find(child => child.tagName === 'SUMMARY');
      if (!summary?.contains(anchor)) return true;
    }
    return false;
  }
  function updateLinks() {
    const existing = new Set();
    for (const anchor of document.querySelectorAll('header a[href], nav a[href], footer a[href]')) {
      if (footer.contains(anchor) || anchor.closest('[hidden]') || !anchor.getClientRects().length || insideClosedContent(anchor)) continue;
      const target = new URL(anchor.href, location.href);
      if (target.origin === location.origin) existing.add(target.pathname.replace(/\/+$/, '') || '/');
    }
    const needed = links.filter(([, href]) => !existing.has(href.replace(/\/+$/, '') || '/'));
    const key = needed.map(([, href]) => href).join('|');
    if (key === renderedLinks) return;
    renderedLinks = key;
    navigation.replaceChildren();
    for (const [index, [title, href]] of needed.entries()) {
      if (index) navigation.append(document.createTextNode(' · '));
      const link = document.createElement('a'); link.textContent = title; link.href = href; link.style.color = 'inherit'; navigation.append(link);
    }
    navigation.hidden = !needed.length;
  }
  updateLinks();
  // Application navigation can render after this shared script has loaded.
  const observer = new MutationObserver(updateLinks);
  observer.observe(document.body, { childList: true, subtree: true, attributes: true, attributeFilter: ['class', 'hidden', 'open', 'aria-expanded'] });
  window.addEventListener('resize', updateLinks, { passive: true });
  if (!adEligible) return;
  let config;
  try { const response = await fetch('/revenue-config.json'); if (!response.ok) return; config = await response.json(); }
  catch { return; }
  const ads = config.adSense;
  if (!ads?.enabled || !/^ca-pub-\d{16}$/.test(ads.client) || !/^\d{5,20}$/.test(ads.slot)) return;
  // Google Privacy & messaging must be configured before ADSENSE_CONSENT_READY is enabled.
  // The publisher's approved consent platform owns region-specific consent decisions.
  const placement = document.createElement('section');
  placement.setAttribute('aria-label', 'Advertisement'); placement.style.cssText = 'margin-top:18px;min-height:90px;overflow:hidden';
  const label = document.createElement('small'); label.textContent = 'Advertisement'; placement.append(label);
  const unit = document.createElement('ins'); unit.className = 'adsbygoogle'; unit.style.display = 'block';
  unit.dataset.adClient = ads.client; unit.dataset.adSlot = ads.slot; unit.dataset.adFormat = 'auto'; unit.dataset.fullWidthResponsive = 'true'; placement.append(unit); footer.append(placement);
  if (!document.querySelector('script[data-binsite-ads]')) {
    const script = document.createElement('script'); script.async = true; script.crossOrigin = 'anonymous'; script.dataset.binsiteAds = 'true';
    script.src = `https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=${encodeURIComponent(ads.client)}`;
    script.addEventListener('load', () => { (window.adsbygoogle = window.adsbygoogle || []).push({}); });
    document.head.append(script);
  } else (window.adsbygoogle = window.adsbygoogle || []).push({});
})();
