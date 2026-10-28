class FDCPHorizontalRadios {
    constructor(element) {
        this.element = element;
        if (this.element.dataset.fdcpHorizontalRadiosInitialized === 'true') {
            return;
        }
        this.element.dataset.fdcpHorizontalRadiosInitialized = 'true';

        this.labels = Array.from(
            this.element.querySelectorAll('.fdcp-horizontal-radios__label')
        );
        this.inputs = Array.from(
            this.element.querySelectorAll(
                '.fdcp-horizontal-radios__input[type="radio"]'
            )
        );
        this.error = this.element.querySelector(
            '.fdcp-horizontal-radios__error'
        );
        this.required = this.inputs.some(input => input.required);
        this.errorMessage = this.element.dataset.requiredMessage
            || this.error?.textContent.trim()
            || 'Choose an option to continue.';
        this.errorSummaryMessage = this.element.dataset.requiredSummaryMessage
            || this.errorMessage;
        this.referenceText = 'sometimes';
        this.animationFrame = null;
        this.measurementFrame = null;
        this.observedWidth = null;
        this.handlingInvalidEvent = false;

        this.resizeObserver = new ResizeObserver(entries => {
            const width = entries[0]?.contentRect.width;

            if (width !== undefined && Math.abs(width - this.observedWidth) < 1) {
                return;
            }

            this.observedWidth = width;
            this.scheduleLayout();
        });
        this.resizeObserver.observe(this.element);
        this.scheduleLayout();

        if (this.required) {
            this.bindValidation();
        }

        if (document.fonts) {
            document.fonts.ready.then(() => this.scheduleLayout());
        }
    }

    bindValidation() {
        this.inputs.forEach(input => {
            input.addEventListener('change', () => {
                if (this.element.classList.contains(
                    'fdcp-horizontal-radios--error'
                )) {
                    this.validate();
                }
            });

            input.addEventListener('invalid', event => {
                event.preventDefault();

                if (this.handlingInvalidEvent) {
                    return;
                }

                this.handlingInvalidEvent = true;
                this.validate({ focus: true });
                queueMicrotask(() => {
                    this.handlingInvalidEvent = false;
                });
            });
        });

        this.element.addEventListener('focusout', event => {
            if (!this.element.contains(event.relatedTarget)) {
                this.validate();
            }
        });

        const form = this.inputs[0]?.form || this.element.closest('form');
        form?.addEventListener('submit', event => {
            if (!this.validate({ focus: true })) {
                event.preventDefault();
            }
        });
    }

    validate({ focus = false } = {}) {
        const enabledInputs = this.inputs.filter(input => !input.disabled);
        const isValid = !this.required
            || enabledInputs.length === 0
            || enabledInputs.some(input => input.checked);

        this.element.classList.toggle(
            'fdcp-horizontal-radios--error',
            !isValid
        );

        if (this.error) {
            this.error.hidden = isValid;
        }

        this.inputs.forEach(input => {
            if (isValid) {
                input.removeAttribute('aria-invalid');
                if (input.getAttribute('aria-describedby') === this.error?.id) {
                    input.removeAttribute('aria-describedby');
                }
            } else {
                input.setAttribute('aria-invalid', 'true');
                if (this.error?.id) {
                    input.setAttribute('aria-describedby', this.error.id);
                }
            }
        });

        this.dispatchValidationEvent(isValid);

        if (!isValid && focus) {
            enabledInputs[0]?.focus();
        }

        return isValid;
    }

    dispatchValidationEvent(isValid) {
        if (isValid) {
            this.element.dispatchEvent(new CustomEvent('gcdsValid', {
                bubbles: true,
                composed: true
            }));
            return;
        }

        this.element.dispatchEvent(new CustomEvent('gcdsError', {
            bubbles: true,
            composed: true,
            detail: {
                message: this.errorSummaryMessage
            }
        }));
    }

    scheduleLayout() {
        if (this.animationFrame !== null) {
            cancelAnimationFrame(this.animationFrame);
        }

        if (this.measurementFrame !== null) {
            cancelAnimationFrame(this.measurementFrame);
        }

        this.animationFrame = requestAnimationFrame(() => {
            this.animationFrame = null;
            this.updateLayout();
        });
    }

    updateLayout() {
        this.setWideLayout(true);

        this.measurementFrame = requestAnimationFrame(() => {
            this.measurementFrame = requestAnimationFrame(() => {
                this.measurementFrame = null;
                this.finishLayoutMeasurement();
            });
        });
    }

    finishLayoutMeasurement() {
        const hasNarrowTextArea = this.labels.some(label => {
            const styles = getComputedStyle(label);
            const availableWidth = label.clientWidth
                - Number.parseFloat(styles.paddingInlineStart || 0)
                - Number.parseFloat(styles.paddingInlineEnd || 0);

            return availableWidth < this.getReferenceTextWidth(styles);
        });

        this.setWideLayout(!hasNarrowTextArea);
    }

    setWideLayout(isWide) {
        this.element.classList.toggle('fdcp-horizontal-radios--wide', isWide);
    }

    getReferenceTextWidth(styles) {
        const canvas = document.createElement('canvas');
        const context = canvas.getContext('2d');

        if (!context) {
            return 96;
        }

        context.font = styles.font || [
            styles.fontWeight,
            styles.fontSize,
            styles.fontFamily
        ].join(' ');

        return Math.ceil(context.measureText(this.referenceText).width);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('[data-fdcp-horizontal-radios]').forEach(element => {
        new FDCPHorizontalRadios(element);
    });
});
