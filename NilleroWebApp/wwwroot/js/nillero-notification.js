/* 
   Connects to /hubs/notifications, keeps the layout badges (#nl-notif-badge,
   #nl-notif-badge-sidebar) in sync in real time, and shows a global toast
   via window.Nillero.toast() whenever a new notification arrives.

   Requires the SignalR browser client to be loaded before this file:
   <script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@8.0.0/dist/browser/signalr.min.js"></script>
*/

(function () {
    "use strict";

    window.Nillero = window.Nillero || {};

    // --- Global toast helper -------------------------------------------------
    // Reusable across the whole app: window.Nillero.toast("Message", "ph-heart")
    window.Nillero.toast = function (message, iconClass) {
        iconClass = iconClass || "ph ph-bell";

        var existing = document.querySelector(".nl-toast");
        if (existing) {
            existing.remove();
        }

        var toast = document.createElement("div");
        toast.className = "nl-toast";
        toast.setAttribute("role", "status");
        toast.setAttribute("aria-live", "polite");
        toast.innerHTML = '<i class="' + iconClass + '" aria-hidden="true"></i><span></span>';
        toast.querySelector("span").textContent = message;

        document.body.appendChild(toast);

        requestAnimationFrame(function () {
            toast.classList.add("nl-toast--visible");
        });

        window.setTimeout(function () {
            toast.classList.remove("nl-toast--visible");
            window.setTimeout(function () {
                toast.remove();
            }, 250);
        }, 4000);
    };

    // --- Badge sync -----------------------------------------------------------
    function updateBadges(count) {
        var badges = [
            document.getElementById("nl-notif-badge"),
            document.getElementById("nl-notif-badge-sidebar")
        ];

        badges.forEach(function (badge) {
            if (!badge) {
                return;
            }

            if (count > 0) {
                badge.textContent = count > 99 ? "99+" : String(count);
                badge.hidden = false;
            } else {
                badge.textContent = "";
                badge.hidden = true;
            }
        });
    }

    // --- Mark-as-read on click (progressive enhancement) ----------------------
    function wireMarkAsRead() {
        document.querySelectorAll("[data-mark-read]").forEach(function (link) {
            link.addEventListener("click", function (e) {
                e.preventDefault(); // detener navegación automática

                var id = link.getAttribute("data-mark-read");
                var href = link.getAttribute("href");
                var item = link.closest(".nl-notification-item");

                if (item) {
                    item.classList.remove("nl-notification-item--unread");
                }

                fetch("/notifications/" + id + "/read", {
                    method: "POST",
                    headers: {
                        "X-Requested-With": "XMLHttpRequest",
                        "RequestVerificationToken": document.querySelector('meta[name="csrf-token"]')?.getAttribute("content") ?? ""
                    }
                })
                    .finally(function () {
                        if (href && href !== "#") {
                            window.location.href = href;
                        }
                    });
            }, { once: true });
        });
    }

    // --- Delete individual notification & Clear all ----------------------------
    function wireNotificationPage() {
        var csrfToken = document.querySelector('meta[name="csrf-token"]')
            ?.getAttribute('content') ?? '';

        function showEmptyStateIfNeeded() {
            var page = document.querySelector('.nl-notif-page');
            if (!page) return;

            // Guard: no insertar si ya existe el empty state
            if (page.querySelector('.nl-notif-empty')) return;
            // Guard: no insertar si todavía quedan notificaciones
            if (page.querySelector('.nl-notification-item')) return;

            page.querySelector('.nl-notif-page__actions')?.remove();
            page.insertAdjacentHTML('beforeend',
                '<div class="nl-notif-empty">' +
                '<i class="ph ph-bell-slash nl-notif-empty__icon" aria-hidden="true"></i>' +
                '<p class="nl-notif-empty__text">You\'re all caught up. New activity will show up here.</p>' +
                '</div>'
            );
        }

        function removeItemFromDOM(item) {
            item.style.maxHeight = item.offsetHeight + 'px';
            item.getBoundingClientRect(); // force reflow
            item.classList.add('is-removing');

            item.addEventListener('transitionend', function () {
                var list = item.closest('.nl-notif-list');
                var section = item.closest('.nl-notif-section');
                item.remove();

                // Remove section if its list is now empty
                if (list && !list.querySelector('.nl-notification-item')) {
                    section?.remove();
                }

                showEmptyStateIfNeeded();
            }, { once: true });
        }

        // Individual delete buttons
        document.querySelectorAll('[data-delete-id]').forEach(function (btn) {
            btn.addEventListener('click', async function (e) {
                e.stopPropagation();
                var id = btn.getAttribute('data-delete-id');
                var item = btn.closest('.nl-notification-item');

                if (item) removeItemFromDOM(item);

                try {
                    await fetch('/notifications/' + id + '/delete', {
                        method: 'POST',
                        headers: {
                            'X-Requested-With': 'XMLHttpRequest',
                            'RequestVerificationToken': csrfToken
                        }
                    });
                } catch { /* non-fatal */ }
            });
        });

        // Clear all button
        var clearAllBtn = document.getElementById('nl-clear-all-btn');
        if (clearAllBtn) {
            clearAllBtn.addEventListener('click', async function () {
                clearAllBtn.disabled = true;
                clearAllBtn.textContent = 'Clearing…';

                try {
                    var res = await fetch('/notifications/delete-all', {
                        method: 'POST',
                        headers: {
                            'X-Requested-With': 'XMLHttpRequest',
                            'RequestVerificationToken': csrfToken
                        }
                    });

                    if (res.ok) {
                        // Collapse all items simultaneously
                        document.querySelectorAll('.nl-notification-item').forEach(function (item) {
                            item.style.maxHeight = item.offsetHeight + 'px';
                            item.getBoundingClientRect();
                            item.classList.add('is-removing');
                        });

                        window.setTimeout(function () {
                            document.querySelectorAll('.nl-notif-section').forEach(function (s) {
                                s.remove();
                            });
                            showEmptyStateIfNeeded();
                        }, 300);
                    }
                } catch {
                    clearAllBtn.disabled = false;
                    clearAllBtn.textContent = 'Clear all';
                }
            });
        }
    }

    // --- SignalR connection ----------------------------------------------------
    function startConnection() {
        if (typeof signalR === "undefined") {
            console.warn("Nillero notifications: SignalR client script not found.");
            return;
        }

        var connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/notifications")
            .withAutomaticReconnect()
            .build();

        connection.on("UpdateUnreadCount", function (count) {
            updateBadges(count);
        });

        connection.on("ReceiveNotification", function (notification) {
            updateBadges(notification.unreadCount);
            window.Nillero.toast(notification.message, notification.iconClass || "ph-fill ph-bell");
            window.Nillero.invalidateDropdown?.();
        });

        connection.start().catch(function (err) {
            console.error("Nillero notifications: connection failed.", err);
        });
    }

    // --- Initialization --------------------------------------------------------
    document.addEventListener("DOMContentLoaded", function () {
        wireMarkAsRead();
        startConnection();
        wireNotificationPage();

        // Initial unread count fetch on page load to prevent staled counters
        if (document.getElementById("nl-notif-badge")) {
            fetch("/notifications/unread-count", {
                headers: { "X-Requested-With": "XMLHttpRequest" }
            })
                .then(function (r) { return r.json(); })
                .then(function (data) { updateBadges(data.count); })
                .catch(function () {
                    /* Non-fatal fallback: standard interface behavior remains intact */
                });
        }
    });

})();

// Notification dropdown toggle
(function () {
    document.addEventListener('DOMContentLoaded', function () {
        var btn = document.getElementById('nl-notif-btn');
        var dropdown = document.getElementById('nl-notif-dropdown');
        var content = document.getElementById('nl-notif-dropdown-content');

        if (!btn || !dropdown || !content) return;

        var loaded = false;

        function getCsrfToken() {
            return document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') ?? '';
        }

        async function loadDropdown() {
            if (loaded) return;
            try {
                var res = await fetch('/notifications/dropdown', {
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });
                if (!res.ok) return;
                content.innerHTML = await res.text();
                loaded = true;
                // Wire up interactions on the freshly injected HTML
                wireDropdownItems();
            } catch {
                content.innerHTML = '<p class="nl-notif-dropdown__empty">Could not load notifications.</p>';
            }
        }

        function wireDropdownItems() {
            // 1. Individual mark-as-read on item click
            content.querySelectorAll('[data-mark-read]').forEach(function (link) {
                link.addEventListener('click', function (e) {
                    e.preventDefault();

                    var id = link.getAttribute('data-mark-read');
                    var href = link.getAttribute('href');
                    var item = link.closest('.nl-notification-item');

                    // Optimistic UI: remove unread state immediately
                    if (item) {
                        item.classList.remove('nl-notification-item--unread');
                        var dot = item.querySelector('.nl-notification-item__dot');
                        if (dot) dot.remove();
                    }

                    fetch('/notifications/' + id + '/read', {
                        method: 'POST',
                        headers: {
                            'X-Requested-With': 'XMLHttpRequest',
                            'RequestVerificationToken': getCsrfToken()
                        }
                    }).finally(function () {
                        // SignalR will update the badge count via UpdateUnreadCount
                        if (href && href !== '#') {
                            window.location.href = href;
                        }
                    });
                }, { once: true });
            });

            // 2. Mark all as read button
            var markAllBtn = content.querySelector('#nl-notif-mark-all');
            if (markAllBtn) {
                markAllBtn.addEventListener('click', async function () {
                    markAllBtn.disabled = true;
                    markAllBtn.textContent = 'Marking…';

                    try {
                        var res = await fetch('/notifications/read-all', {
                            method: 'POST',
                            headers: {
                                'X-Requested-With': 'XMLHttpRequest',
                                'RequestVerificationToken': getCsrfToken()
                            }
                        });

                        if (res.ok) {
                            // Remove all unread visual states in the dropdown
                            content.querySelectorAll('.nl-notification-item--unread').forEach(function (item) {
                                item.classList.remove('nl-notification-item--unread');
                                var dot = item.querySelector('.nl-notification-item__dot');
                                if (dot) dot.remove();
                            });
                            // Remove the button itself — nothing left to mark
                            markAllBtn.remove();
                            // Badge goes to 0 via SignalR (UpdateUnreadCount),
                            // but force it client-side too in case SignalR is slow
                            updateBadges(0);
                        }
                    } catch {
                        markAllBtn.disabled = false;
                        markAllBtn.textContent = 'Mark all as read';
                    }
                });
            }
        }

        btn.addEventListener('click', async function (e) {
            e.stopPropagation();
            var isOpen = !dropdown.hidden;

            if (isOpen) {
                dropdown.hidden = true;
                btn.setAttribute('aria-expanded', 'false');
            } else {
                dropdown.hidden = false;
                btn.setAttribute('aria-expanded', 'true');
                await loadDropdown();
            }
        });

        document.addEventListener('click', function (e) {
            if (!dropdown.hidden && !dropdown.contains(e.target) && e.target !== btn) {
                dropdown.hidden = true;
                btn.setAttribute('aria-expanded', 'false');
            }
        });

        window.Nillero = window.Nillero || {};
        window.Nillero.invalidateDropdown = function () { loaded = false; };
    });
})();