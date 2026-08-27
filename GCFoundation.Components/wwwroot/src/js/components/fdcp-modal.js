class FDCPModal {
    constructor(element) {
        this.modal = element;
        this.isStatic = element.dataset.static === 'true';
        this.closeButtons = element.querySelectorAll('.fdcp-modal-close');
        this.triggerElement = null;
        this.table = document.querySelector('gcds-table');
        this.bindEvents();
    }

    bindEvents() {
        this.closeButtons.forEach(btn => {
            btn.addEventListener('click', () => this.hide());
        });

        // Native <dialog> puts backdrop clicks on the dialog element itself
        // (there's no separate backdrop node anymore), so distinguish a
        // click on the dialog's own padding/backdrop area from a click
        // inside its content.
        this.modal.addEventListener('click', (e) => {
            if (this.isStatic) return;
            if (e.target === this.modal) {
                this.hide();
            }
        });

        // ESC triggers 'cancel' before 'close'. Block it here for static
        // backdrop modals instead of in a document-level keydown handler.
        this.modal.addEventListener('cancel', (e) => {
            if (this.isStatic) {
                e.preventDefault();
            }
        });

        // Fires on any close path: close(), cancel->close, or a future
        // <form method="dialog"> submission — so this is the single place
        // exit cleanup belongs, regardless of how the modal was closed.
        this.modal.addEventListener('close', () => this.onExit());
    }

    onEnter() {
        document.body.style.overflow = 'hidden';

        const focusTarget =
            this.modal.querySelector('.fdcp-modal-close') ||
            this.modal.querySelector('.modal__body') ||
            this.modal.querySelector('.modal__footer gcds-button, .modal__footer button') ||
            this.modal;

        requestAnimationFrame(() => {
            if (typeof focusTarget.focus === 'function') {
                focusTarget.focus();
            }
        });
    }

    onExit() {
        document.body.style.overflow = '';

        if (this.triggerElement && typeof this.triggerElement.focus === 'function') {
            this.triggerElement.focus();
        }
        this.triggerElement = null;
    }

    show(triggerElement) {
        this.triggerElement = triggerElement || document.activeElement;
        this.modal.showModal();
        this.onEnter();
    }

    hide() {
        this.modal.close();
        // onExit() fires automatically via the 'close' event
    }
}
function bindModalTrigger(trigger) {
    if (trigger.dataset.fdcpBound) return;
    trigger.addEventListener('click', () => {
        const targetId = trigger.getAttribute('modal-id');
        const instance = fdcpModalRegistry.get(targetId);
        if (instance) {
            instance.show(trigger);
        }
    });
    trigger.dataset.fdcpBound = 'true';
}

const fdcpModalRegistry = new Map();

document.addEventListener('DOMContentLoaded', () => {
    const tables = document.querySelectorAll('gcds-table');
    if (tables.length >= 1) {
        document.addEventListener('fdcp-table:rows-rendered', (e) => {
            if (!Array.from(tables).includes(e.detail.table)) return;
            e.detail.table.querySelectorAll('.fdcp-modal-open[modal-id]').forEach(bindModalTrigger);
        });
    }
    document.querySelectorAll('dialog.modal').forEach(modalEl => {
        fdcpModalRegistry.set(modalEl.getAttribute('modal-id'), new FDCPModal(modalEl));
    });

    document.querySelectorAll('.fdcp-modal-open[modal-id]').forEach(trigger => {
        trigger.addEventListener('click', () => {
            const targetId = trigger.getAttribute('modal-id');
            const instance = fdcpModalRegistry.get(targetId);
            if (instance) {
                instance.show(trigger);
            }
        });
    });
});