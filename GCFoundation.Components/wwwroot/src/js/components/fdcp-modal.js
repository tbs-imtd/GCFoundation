class FDCPModal {
    constructor(element) {
        this.modal = element;
        this.isStatic = element.dataset.static === 'true';
        this.closeButtons = element.querySelectorAll('.fdcp-modal-close');
        this.triggerElement = null;
        this.table = document.querySelector('gcds-table');
        this.keydownHandler = (e) => this.onKeydown(e);
        this.bindEvents();
    }

    // Collect every focusable element inside root (in DOM order), descending
    // into shadow roots so GCDS buttons expose their inner <button>/<a>.
    // Uses the tabIndex property rather than a list of element types.
    getFocusableElements(root = this.modal) {
        const result = [];
        const walk = (node) => {
            for (const el of node.children) {
                if (this.isFocusable(el)) result.push(el);
                if (el.shadowRoot) walk(el.shadowRoot);
                walk(el); // light-DOM children (slotted content)
            }
        };
        walk(root);
        return result;
    }

    isFocusable(el) {
        // Chrome reports tabIndex -1 for a contenteditable host with no
        // tabindex attribute (e.g. Quill's .ql-editor) even though Tab
        // reaches it, so treat that case as focusable.
        const editableHost = !el.hasAttribute('tabindex') &&
            ['', 'true', 'plaintext-only'].includes(el.getAttribute('contenteditable'));
        if ((el.tabIndex < 0 && !editableHost) || el.disabled || el.hidden) return false;
        if (el.closest?.('[inert]')) return false;
        const visible = typeof el.checkVisibility === 'function'
            ? el.checkVisibility({ visibilityProperty: true })
            : el.getClientRects().length > 0;
        if (!visible) return false;
        return this.isRadioTabStop(el);
    }

    // Every radio has tabIndex 0, but the browser only tab-stops on one per
    // group (the checked one, else the first). Mirror that so wrapping uses
    // the elements Tab can actually reach.
    isRadioTabStop(el) {
        if (el.type !== 'radio' || !el.name) return true;
        const group = Array.from(
            el.getRootNode().querySelectorAll('input[type="radio"]')
        ).filter(r => r.name === el.name && r.form === el.form && !r.disabled);
        const checked = group.find(r => r.checked);
        return (checked || group[0]) === el;
    }

    onKeydown(e) {
        if (e.key !== 'Tab' || e.altKey || e.ctrlKey || e.metaKey) return;

        const focusable = this.getFocusableElements();
        if (focusable.length === 0) {
            e.preventDefault();
            this.modal.focus();
            return;
        }

        const first = focusable[0];
        const last = focusable[focusable.length - 1];
        // Deepest active element, through shadow roots
        let active = document.activeElement;
        while (active?.shadowRoot?.activeElement) active = active.shadowRoot.activeElement;

        const isFirst = active === first || active === this.modal;
        const isLast = active === last;

        if (e.shiftKey && isFirst) {
            e.preventDefault();
            last.focus();
        } else if (!e.shiftKey && isLast) {
            e.preventDefault();
            first.focus();
        }
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
        this.modal.addEventListener('keydown', this.keydownHandler);
        this.getInitialFocusTarget().focus();
    }

    getInitialFocusTarget() {
        return (
            this.modal.querySelector('.fdcp-modal-close') ||
            this.modal.querySelector('.modal__footer gcds-button, .modal__footer button') ||
            this.modal
        );
    }

    onExit() {
        this.modal.removeEventListener('keydown', this.keydownHandler);
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