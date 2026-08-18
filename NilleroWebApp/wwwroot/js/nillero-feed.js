(function () {
    'use strict';

    /* ── Auto-expanding textareas ── */
    function autoExpand(el) {
        el.style.height = 'auto';
        el.style.height = el.scrollHeight + 'px';
    }

    document.querySelectorAll('.nl-add-comment-form__input').forEach(function (ta) {
        ta.addEventListener('input', function () { autoExpand(this); });
    });

    /* ── Reply form toggle ── */
    document.querySelectorAll('[data-reply-trigger]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var formId = this.dataset.replyTrigger;
            var form = document.getElementById(formId);
            if (!form) return;

            var isOpen = !form.hidden;
            form.hidden = isOpen;
            this.setAttribute('aria-expanded', isOpen ? 'false' : 'true');

            if (!isOpen) {
                var ta = form.querySelector('textarea');
                if (ta) { ta.focus(); autoExpand(ta); }
            }
        });
    });

    /* ── Edit comment toggle ── */
    document.querySelectorAll('[data-edit-trigger]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var formId = this.dataset.editTrigger;
            var form = document.getElementById(formId);
            var contentId = formId.replace('comment-edit-', 'comment-content-');
            var content = document.getElementById(contentId);
            if (!form) return;

            form.hidden = !form.hidden;
            if (content) content.hidden = !content.hidden;

            if (!form.hidden) {
                var ta = form.querySelector('textarea');
                if (ta) { ta.focus(); autoExpand(ta); }
            }
        });
    });

    /* ── Cancel edit ── */
    document.querySelectorAll('[data-edit-cancel]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var formId = this.dataset.editCancel;
            var form = document.getElementById(formId);
            var contentId = formId.replace('comment-edit-', 'comment-content-');
            var content = document.getElementById(contentId);
            if (form) form.hidden = true;
            if (content) content.hidden = false;
        });
    });

    /* ── Scroll to comment from notification ── */
    var hash = window.location.hash;

    if (hash && hash.startsWith("#comment-")) {

        var target = document.querySelector(hash);

        if (target) {

            var details = target.closest("details");
            if (details)
                details.open = true;

            setTimeout(function () {

                target.scrollIntoView({
                    behavior: "smooth",
                    block: "center"
                });

                target.classList.add("nl-comment--highlight");

                setTimeout(function () {
                    target.classList.remove("nl-comment--highlight");
                }, 2500);

            }, 150);
        }
    }

})();

