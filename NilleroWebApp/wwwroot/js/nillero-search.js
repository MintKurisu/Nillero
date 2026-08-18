// nillero-search.js
(function () {
    'use strict';

    const input = document.getElementById('nl-search-input');
    const wrapper = document.getElementById('nl-search-wrapper');
    const dropdown = document.getElementById('nl-search-dropdown');
    const results = document.getElementById('nl-search-results');

    if (!input || !dropdown || !results) return;

    let debounceTimer = null;
    let currentQuery = '';

    // ── Helpers ──────────────────────────────────────────────

    function escapeHtml(str) {
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function highlight(text, term) {
        if (!term) return escapeHtml(text);
        const escaped = term.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
        const re = new RegExp(`(${escaped})`, 'gi');
        return escapeHtml(text).replace(re, '<mark class="nl-highlight">$1</mark>');
    }

    function avatarImg(src, alt) {
        const safe = escapeHtml(alt);
        const fallback = '/img/avatar-placeholder.webp';
        const url = src ? escapeHtml(src) : fallback;
        return `<img src="${url}" alt="${safe}" class="nl-search-item__avatar" width="32" height="32" loading="lazy">`;
    }

    // ── Render ───────────────────────────────────────────────

    function renderResults(data, query) {
        const { people, posts } = data;

        if (!people.length && !posts.length) {
            results.innerHTML = `<p class="nl-search-empty">No results for <strong>${escapeHtml(query)}</strong></p>`;
            return;
        }

        let html = '';

        if (people.length) {
            html += `<div class="nl-search-section">
                <span class="nl-search-section__label">
                    <i class="ph ph-users" aria-hidden="true"></i> People
                </span>`;

            people.forEach(p => {
                // ProfileController.Index() only serves the current user's own profile.
                // For viewing another user's posts, FriendsController.UserPosts is the
                // correct route — it also validates the friendship before rendering.
                const profileUrl = `/Friends/UserPosts?userId=${encodeURIComponent(p.userId)}`;

                html += `
                <a href="${profileUrl}" class="nl-search-item" data-search-item>
                    ${avatarImg(p.avatar, p.fullName)}
                    <div class="nl-search-item__body">
                        <div class="nl-search-item__name">${highlight(p.fullName, query)}</div>
                        <div class="nl-search-item__meta">@${highlight(p.userName, query)}</div>
                    </div>
                    <i class="ph ph-arrow-right" aria-hidden="true"
                       style="color:var(--nl-text-3);font-size:14px;flex-shrink:0;"></i>
                </a>`;
            });

            html += `</div>`;
        }

        if (people.length && posts.length) {
            html += `<div class="nl-search-divider"></div>`;
        }

        if (posts.length) {
            html += `<div class="nl-search-section">
                <span class="nl-search-section__label">
                    <i class="ph ph-article" aria-hidden="true"></i> Posts
                </span>`;

            posts.forEach(p => {
                // HomeController.Details(int id) is the correct post detail route.
                const postUrl = `/Home/Details/${p.postId}`;

                html += `
                <a href="${postUrl}" class="nl-search-item" data-search-item>
                    ${avatarImg(p.avatar, p.authorName)}
                    <div class="nl-search-item__body">
                        <div class="nl-search-item__name">
                            ${escapeHtml(p.authorName)}
                            <span style="font-weight:400;color:var(--nl-text-3)"> · ${escapeHtml(p.createdAt)}</span>
                        </div>
                        <div class="nl-search-item__meta">${highlight(p.snippet, query)}</div>
                    </div>
                </a>`;
            });

            html += `</div>`;
        }

        html += `<a href="/Search?q=${encodeURIComponent(query)}" class="nl-search-footer">
            See all results for "${escapeHtml(query)}"
            <i class="ph ph-arrow-right" aria-hidden="true"></i>
        </a>`;

        results.innerHTML = html;
    }

    function showDropdown() {
        dropdown.hidden = false;
        // rAF ensures the CSS transition fires after the element is made visible
        requestAnimationFrame(() => dropdown.classList.add('is-visible'));
        input.setAttribute('aria-expanded', 'true');
    }

    function hideDropdown() {
        dropdown.classList.remove('is-visible');
        input.setAttribute('aria-expanded', 'false');
        // Wait for the opacity/transform transition before removing from DOM flow
        dropdown.addEventListener('transitionend', () => {
            dropdown.hidden = true;
        }, { once: true });
    }

    // ── Fetch ────────────────────────────────────────────────

    async function fetchResults(query) {
        try {
            const res = await fetch(`/Search/Live?q=${encodeURIComponent(query)}`, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            if (!res.ok) return;
            const data = await res.json();
            renderResults(data, query);
            showDropdown();
        } catch {
            // Silent fail — search is non-critical, never break the rest of the UI
        }
    }

    // ── Events ───────────────────────────────────────────────

    input.addEventListener('input', function () {
        const q = this.value.trim();
        clearTimeout(debounceTimer);

        if (q.length < 2) {
            hideDropdown();
            return;
        }

        currentQuery = q;
        results.innerHTML = '';

        debounceTimer = setTimeout(() => {
            if (currentQuery === q) fetchResults(q);
        }, 280);
    });

    input.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            const q = this.value.trim();
            if (q.length >= 2) {
                window.location.href = `/Search?q=${encodeURIComponent(q)}`;
            }
        }
        if (e.key === 'Escape') {
            hideDropdown();
            input.blur();
        }
    });

    document.addEventListener('click', function (e) {
        if (!wrapper.contains(e.target)) hideDropdown();
    });

    results.addEventListener('click', function (e) {
        if (e.target.closest('[data-search-item]')) hideDropdown();
    });

})();