// Sidebar toggle for small screens.
(function () {
  const body = document.body;
  const toggler = document.getElementById('iconNavbarSidenav');
  const closeIcon = document.getElementById('iconSidenav');
  const sidenav = document.getElementById('sidenav-main');
  if (!toggler || !sidenav) return;

  function open() {
    body.classList.add('g-sidenav-pinned');
    ensureBackdrop();
  }
  function close() {
    body.classList.remove('g-sidenav-pinned');
    removeBackdrop();
  }
  function toggle() {
    if (body.classList.contains('g-sidenav-pinned')) {
      close();
    } else {
      open();
    }
  }

  let backdrop = null;
  function ensureBackdrop() {
    if (backdrop) return;
    backdrop = document.createElement('div');
    backdrop.className = 'sidenav-backdrop';
    backdrop.addEventListener('click', close);
    document.body.appendChild(backdrop);
  }
  function removeBackdrop() {
    if (backdrop) {
      backdrop.remove();
      backdrop = null;
    }
  }

  toggler.addEventListener('click', function (e) {
    e.preventDefault();
    toggle();
  });
  if (closeIcon) {
    closeIcon.addEventListener('click', close);
  }

  // Close when the viewport grows back to desktop width.
  window.addEventListener('resize', function () {
    if (window.innerWidth >= 1200) close();
  });
})();
