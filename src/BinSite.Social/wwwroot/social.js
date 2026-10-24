window.binSiteSocial = Object.freeze({
  getLanguage: () => localStorage.getItem('binsite-social-language'),
  setLanguage: language => { localStorage.setItem('binsite-social-language', language); document.documentElement.lang = language; },
  copy: async text => { try { await navigator.clipboard.writeText(text); return true; } catch { return false; } },
  localDate: () => new Date().toISOString(),
  replaceUrl: url => history.replaceState(null, '', url)
});
