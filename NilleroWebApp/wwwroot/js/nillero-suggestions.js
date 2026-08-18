// wwwroot/js/nillero-suggestions.js
(function () {
    const slot = document.getElementById('nl-suggestions-slot');
    if (!slot) return;

    async function loadSuggestions() {
        try {
            const res = await fetch('/FriendRequest/Suggestions');
            if (!res.ok) throw new Error('fetch failed');
            const users = await res.json();

            if (!users.length) {
                slot.innerHTML = '<p class="nl-text-muted nl-text-sm">No suggestions right now.</p>';
                return;
            }

            slot.innerHTML = users.map(u => `
        <div class="nl-suggestion-card" style="--enter-delay: ${users.indexOf(u) * 80}ms">
          <img src="${u.profilePicturePath || '/img/avatar-placeholder.webp'}"
               alt="${u.fullName}"
               class="nl-avatar nl-avatar--sm"
               width="36" height="36" loading="lazy" />
          <div class="nl-suggestion-card__info">
            <span class="nl-suggestion-card__name">${u.fullName}</span>
            <span class="nl-suggestion-card__handle">@${u.userName}</span>
            ${u.mutualFriendsCount > 0
                    ? `<span class="nl-suggestion-card__mutual">
                   <i class="ph ph-users" aria-hidden="true"></i>
                   ${u.mutualFriendsCount} mutual
                 </span>`
                    : ''}
          </div>
          <form method="post" action="/FriendRequest/SendRequest" style="margin:0">
            <input type="hidden" name="__RequestVerificationToken"
                   value="${document.querySelector('meta[name=csrf-token]')?.content ?? ''}" />
            <input type="hidden" name="selectedUserId" value="${u.userId}" />
            <button type="submit" class="nl-btn nl-btn--sm nl-btn--ghost nl-suggestion-card__btn"
                    aria-label="Add ${u.fullName}">
              <i class="ph ph-user-plus" aria-hidden="true"></i>
            </button>
          </form>
        </div>
      `).join('');

        } catch {
            slot.innerHTML = '<p class="nl-text-muted nl-text-sm">Could not load suggestions.</p>';
        }
    }

    loadSuggestions();
})();