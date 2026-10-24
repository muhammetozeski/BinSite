window.binSiteSocial = Object.freeze({
  getLanguage: () => localStorage.getItem('binsite-social-language'),
  setLanguage: language => { localStorage.setItem('binsite-social-language', language); document.documentElement.lang = language; },
  copy: async text => { try { await navigator.clipboard.writeText(text); return true; } catch { return false; } },
  localDate: () => new Date().toISOString(),
  replaceUrl: url => history.replaceState(null, '', url),
  focusField: id => {
    const field = document.getElementById(id);
    if (field) {
      for (let details = field.closest('details'); details; details = details.parentElement?.closest('details')) details.open = true;
      field.focus({ preventScroll: true }); field.scrollIntoView({ block: 'center' });
    }
  },
  downloadProject: (title, contents) => {
    const filename = String(title || 'project').replace(/[<>:"/\\|?*\x00-\x1f]/g, '_').trim().slice(0, 80) || 'project';
    const url = URL.createObjectURL(new Blob([contents], { type: 'application/json' }));
    const anchor = document.createElement('a');
    anchor.href = url; anchor.download = filename + '.json'; anchor.click();
    setTimeout(() => URL.revokeObjectURL(url), 0);
  }
});
