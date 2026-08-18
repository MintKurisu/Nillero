/* ============================================================
   NILLERO — Base JS
   Responsabilidades:
   - Sidebar off-canvas (mobile)
   - Avatar dropdown
   - Toast auto-dismiss
   - Scroll-reveal (IntersectionObserver)
   - Notification badge helper
   ============================================================ */

(function () {
    'use strict';

    /* ── DOM refs ── */
    const sidebar = document.getElementById('nl-sidebar');
    const sidebarToggle = document.getElementById('nl-sidebar-toggle');
    const sidebarOverlay = document.getElementById('nl-sidebar-overlay');
    const avatarTrigger = document.getElementById('nl-avatar-trigger');
    const avatarDropdown = document.getElementById('nl-avatar-dropdown');
    const toastRegion = document.getElementById('nl-toast-region');

    /* ────────────────────────────────────────────────────────
       1. SIDEBAR OFF-CANVAS
    ──────────────────────────────────────────────────────── */
    function openSidebar() {
        sidebar?.classList.add('is-open');
        sidebarOverlay?.classList.add('is-visible');
        sidebarToggle?.setAttribute('aria-expanded', 'true');
        document.body.style.overflow = 'hidden';
    }

    function closeSidebar() {
        sidebar?.classList.remove('is-open');
        sidebarOverlay?.classList.remove('is-visible');
        sidebarToggle?.setAttribute('aria-expanded', 'false');
        document.body.style.overflow = '';
    }

    sidebarToggle?.addEventListener('click', () => {
        const isOpen = sidebar?.classList.contains('is-open');
        isOpen ? closeSidebar() : openSidebar();
    });

    sidebarOverlay?.addEventListener('click', closeSidebar);

    /* Close sidebar on nav link click (mobile UX) */
    sidebar?.querySelectorAll('.nl-nav__link').forEach(link => {
        link.addEventListener('click', closeSidebar);
    });

    /* Close on Escape */
    document.addEventListener('keydown', e => {
        if (e.key === 'Escape') {
            closeSidebar();
            closeDropdown();
        }
    });

    /* ────────────────────────────────────────────────────────
       2. AVATAR DROPDOWN
    ──────────────────────────────────────────────────────── */
    function openDropdown() {
        avatarDropdown?.removeAttribute('hidden');
        avatarTrigger?.setAttribute('aria-expanded', 'true');
    }

    function closeDropdown() {
        avatarDropdown?.setAttribute('hidden', '');
        avatarTrigger?.setAttribute('aria-expanded', 'false');
    }

    avatarTrigger?.addEventListener('click', e => {
        e.stopPropagation();
        const isOpen = !avatarDropdown?.hasAttribute('hidden');
        isOpen ? closeDropdown() : openDropdown();
    });

    document.addEventListener('click', e => {
        if (!avatarDropdown?.hasAttribute('hidden') &&
            !document.getElementById('nl-avatar-menu')?.contains(e.target)) {
            closeDropdown();
        }
    });

    /* ────────────────────────────────────────────────────────
       3. TOASTS
    ──────────────────────────────────────────────────────── */
    const TOAST_DURATION = 4000;
    const TOAST_ANIM_OUT = 250;

    /**
     * Mostrar un toast programático.
     * Expuesto como window.Nillero.toast(message, type)
     * @param {string} message
     * @param {'success'|'error'|'info'} type
     */
    function showToast(message, type = 'info') {
        const icons = {
            success: 'ph-fill ph-check-circle',
            error: 'ph-fill ph-x-circle',
            info: 'ph ph-info',
        };

        const toast = document.createElement('div');
        toast.className = `nl-toast nl-toast--${type}`;
        toast.setAttribute('role', type === 'error' ? 'alert' : 'status');
        toast.innerHTML = `<i class="${icons[type] ?? icons.info}" aria-hidden="true"></i><span>${message}</span>`;

        toastRegion?.appendChild(toast);

        // Trigger entrada
        requestAnimationFrame(() => {
            requestAnimationFrame(() => toast.classList.add('is-visible'));
        });

        // Auto-dismiss
        setTimeout(() => {
            toast.classList.add('is-leaving');
            toast.classList.remove('is-visible');
            setTimeout(() => toast.remove(), TOAST_ANIM_OUT);
        }, TOAST_DURATION);
    }

    /* Auto-dismiss toasts renderizados server-side (TempData) */
    document.querySelectorAll('.nl-toast.is-visible').forEach(toast => {
        setTimeout(() => {
            toast.classList.add('is-leaving');
            toast.classList.remove('is-visible');
            setTimeout(() => toast.remove(), TOAST_ANIM_OUT);
        }, TOAST_DURATION);
    });

    /* ────────────────────────────────────────────────────────
       4. SCROLL REVEAL (IntersectionObserver)
    ──────────────────────────────────────────────────────── */
    const revealObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('is-visible');
                revealObserver.unobserve(entry.target);
            }
        });
    }, { threshold: 0.08 });

    document.querySelectorAll('.nl-reveal').forEach(el => revealObserver.observe(el));

    /* ────────────────────────────────────────────────────────
       5. NOTIFICATION BADGE HELPER
       Consumido por el hub de SignalR cuando esté listo.
       Uso:  window.Nillero.setNotifCount(5)
    ──────────────────────────────────────────────────────── */
    const badgeIds = ['nl-notif-badge', 'nl-notif-badge-sidebar', 'nl-notif-badge-bottom'];

    function setNotifCount(count) {
        const n = parseInt(count, 10) || 0;
        const label = n > 99 ? '99+' : String(n);

        badgeIds.forEach(id => {
            const el = document.getElementById(id);
            if (!el) return;
            if (n > 0) {
                el.textContent = label;
                el.removeAttribute('hidden');
            } else {
                el.setAttribute('hidden', '');
            }
        });
    }

    /* ────────────────────────────────────────────────────────
       6. PUBLIC API
    ──────────────────────────────────────────────────────── */
    window.Nillero = {
        toast: showToast,
        setNotifCount,
        openSidebar,
        closeSidebar,
    };

})();