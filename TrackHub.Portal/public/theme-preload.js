// Paints the last-known theme background before the bundle loads. The real theme only arrives with
// the user settings query, so without this a dark-profile user gets a white page for the whole load
// and then a visible flip. Mirrors src/utils/uiPreferences.ts (storage key + `darkMode` field) and
// the two `background.default` values in src/assets/theme{,-dark}/base/colors.ts.
// A file rather than an inline script so the page needs no script-src exception in the CSP.
(function () {
  try {
    var stored = window.localStorage.getItem("trackhub.ui-preferences");
    if (!stored) return;
    var dark = JSON.parse(stored).darkMode === true;
    document.documentElement.style.backgroundColor = dark ? "#0A111C" : "#F3F5F8";
    document.documentElement.style.colorScheme = dark ? "dark" : "light";
  } catch (e) {
    // Storage blocked or payload corrupt - fall through to the default shell.
  }
})();
