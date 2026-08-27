// Custom scripts for GCFoundationWeb
const GCFoundationWeb = {
    Version: "0.1.1",

    init: function () {
        this.initCreateCopyButton();
        this.initCopy();
        this.initTableActions();
        this.initDeleteModal();
    },
    initTableActions: function () {
        document.addEventListener("click", (e) => {
            const deleteBtn = e.target.closest('.delete');
            if (deleteBtn) {
                e.preventDefault();
                console.log("Delete clicked for row:", deleteBtn.row, "submisisonId:", deleteBtn.row?.submissionId);
                return;
            }

            const addBtn = e.target.closest('.add-submission');
            if (addBtn) {
                e.preventDefault();
                console.log("Add submission clicked");
            }
        });
    },
    initDeleteModal: function () {
        document.addEventListener("click", (e) => {
            const trigger = e.target.closest('.fdcp-modal-open');
            if (!trigger) return;

            const employeeId = trigger.getAttribute('data-employee-id');
            const employeeName = trigger.getAttribute('data-employee-name');
            const modalId = trigger.getAttribute('modal-id');

            const modal = document.querySelector(`dialog[modal-id="${modalId}"]`);
            if (!modal) return;

            const modalContent = modal.querySelector('.modal__body')
            if (!modalContent) return;

            const modalFooter = modal.querySelector('.modal__footer')
            if (!modalFooter) return;

            const submitBtn = modalFooter.querySelector('gcds-button[button-id=delete-employee]');
            if (submitBtn) {
                submitBtn['value'] = employeeId;
            }

            const nameTarget = modalContent.querySelector('strong[data-employee-name-display]');
            if (nameTarget) {
                nameTarget.textContent = employeeName;
            }
        });
    },
    initCopy: function () {
        document.addEventListener("click", (e) => {
            const copyButton = e.target.closest('.code-copy-button');
            if (!copyButton) return;

            const container = copyButton.closest('pre') || copyButton.parentElement;
            const codeEl = container.querySelector('code');
            if (!codeEl) return;

            const text = codeEl.textContent;
            navigator.clipboard.writeText(text).then(() => {
                const originalText = copyButton.textContent;
                const successText = copyButton.dataset.successText || 'Copied!';
                copyButton.textContent = successText;
                setTimeout(() => {
                    copyButton.textContent = originalText;
                }, 1000);
            });
        });
    },
    initCreateCopyButton: function () {
        const lang = document.documentElement.lang?.startsWith('fr') ? 'fr' : 'en';
        const strings = COPY_BUTTON_STRINGS[lang];

        document.querySelectorAll('pre:has(code)').forEach(pre => {
            if (pre.querySelector('.code-copy-button')) return;

            const button = document.createElement('gcds-button');
            button.setAttribute('button-role', 'secondary');
            button.classList.add('code-copy-button', 'mt-150');
            button.setAttribute('data-success-text', strings.copied);
            button.textContent = strings.copy;

            pre.appendChild(button);
        });
    }
};

const COPY_BUTTON_STRINGS = {
    en: { copy: 'Copy', copied: 'Copied!' },
    fr: { copy: 'Copier', copied: 'Copié!' }
};

// Initialize custom functionality when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    console.log('Custom scripts loaded for GCFoundationWeb');

    // Add any custom initialization here
    GCFoundationWeb.init();
});