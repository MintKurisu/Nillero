/* 
    Native tab switching, comment editing, replies, and hash-scrolling for the Friends hub. 
    No framework, pure vanilla-JS.
*/

(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {

        // --- 1. Tab Switching ---
        var triggers = document.querySelectorAll("[data-tab-target]");

        triggers.forEach(function (trigger) {
            trigger.addEventListener("click", function () {
                var targetId = trigger.getAttribute("data-tab-target");

                triggers.forEach(function (t) {
                    var isActive = t === trigger;
                    t.classList.toggle("nl-tabs__trigger--active", isActive);
                    t.setAttribute("aria-selected", String(isActive));
                });

                document.querySelectorAll(".nl-tabs__panel").forEach(function (panel) {
                    var isActive = panel.id === targetId;
                    panel.classList.toggle("nl-tabs__panel--active", isActive);
                    panel.hidden = !isActive;
                });
            });
        });

        // --- 2. Comment Management (Edit, Cancel, and Reply) ---
        document.addEventListener("click", function (event) {

            // A. Edit Comment Click
            var editButton = event.target.closest("[data-edit-trigger]");
            if (editButton) {
                event.preventDefault();
                var formId = editButton.getAttribute("data-edit-trigger");
                var editForm = document.getElementById(formId);

                if (editForm) {
                    var commentId = formId.replace("comment-edit-", "");
                    var contentDiv = document.getElementById("comment-content-" + commentId);

                    // Show edit form, hide original text
                    editForm.removeAttribute("hidden");
                    if (contentDiv) {
                        contentDiv.style.display = "none";
                    }

                    // Focus textarea and place cursor at the end of the text
                    var textarea = editForm.querySelector("textarea");
                    if (textarea) {
                        textarea.focus();
                        var length = textarea.value.length;
                        textarea.setSelectionRange(length, length);
                    }
                }
                return;
            }

            // B. Cancel Edit Click
            var cancelButton = event.target.closest("[data-edit-cancel]");
            if (cancelButton) {
                event.preventDefault();
                var formId = cancelButton.getAttribute("data-edit-cancel");
                var editForm = document.getElementById(formId);

                if (editForm) {
                    var commentId = formId.replace("comment-edit-", "");
                    var contentDiv = document.getElementById("comment-content-" + commentId);

                    // Hide edit form, show original text again
                    editForm.setAttribute("hidden", "true");
                    if (contentDiv) {
                        contentDiv.style.display = "block";
                    }
                }
                return;
            }

            // C. Reply Click
            var replyButton = event.target.closest("[data-reply-trigger]");
            if (replyButton) {
                event.preventDefault();
                var formId = replyButton.getAttribute("data-reply-trigger");
                var replyForm = document.getElementById(formId);

                if (replyForm) {
                    var isHidden = replyForm.hasAttribute("hidden");
                    if (isHidden) {
                        replyForm.removeAttribute("hidden");
                        replyButton.setAttribute("aria-expanded", "true");
                        var textarea = replyForm.querySelector("textarea");
                        if (textarea) {
                            textarea.focus();
                        }
                    } else {
                        replyForm.setAttribute("hidden", "true");
                        replyButton.setAttribute("aria-expanded", "false");
                    }
                }
            }
        });

        // --- 3. Auto-expand comments on load with a URL Hash (#) ---
        if (window.location.hash) {
            var targetDetails = document.querySelector(window.location.hash);
            if (targetDetails && targetDetails.tagName === "DETAILS") {
                targetDetails.setAttribute("open", "true");
                targetDetails.scrollIntoView({ behavior: "smooth", block: "center" });
            }
        }

    });
})();