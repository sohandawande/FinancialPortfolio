/**
 * Theme bootstrap — runs before Angular.
 * Must stay in sync with ThemeService STORAGE_KEY = 'fp-theme-mode'
 */
(function () {
  try {
    var mode = localStorage.getItem('fp-theme-mode') || 'system';
    var prefersDark =
      window.matchMedia &&
      window.matchMedia('(prefers-color-scheme: dark)').matches;

    var resolved =
      mode === 'dark' || (mode === 'system' && prefersDark) ? 'dark' : 'light';

    document.documentElement.setAttribute('data-theme', resolved);
    document.documentElement.setAttribute('data-bs-theme', resolved);
  } catch (e) {
    // private mode / blocked storage — keep default light
  }
})();