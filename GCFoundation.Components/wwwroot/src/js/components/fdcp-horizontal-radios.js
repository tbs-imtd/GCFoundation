class FDCPHorizontalRadios {
    constructor(element) {
        this.element = element;
        if (this.element.dataset.fdcpHorizontalRadiosInitialized === 'true') {
            return;
        }
        this.element.dataset.fdcpHorizontalRadiosInitialized = 'true';

        this.inputs = Array.from(
            this.element.querySelectorAll(
                '.fdcp-horizontal-radios__input[type="radio"]'
            )
        );
        this.error = this.element.querySelector(
            '.fdcp-horizontal-radios__error'
        );
        this.labelTexts = Array.from(
            this.element.querySelectorAll(
                '.fdcp-horizontal-radios__label-text'
            )
        );
        this.required = this.inputs.some(input => input.required);
        this.errorSummaryMessage = this.element.dataset.requiredSummaryMessage
            || this.error?.textContent.trim()
            || 'Choose an option to continue.';
        this.handlingInvalidEvent = false;

        if (this.required) {
            this.bindValidation();
        }

        this.bindCardSelection();
        this.bindResponsiveLabelLayout();
    }

    bindCardSelection() {
        this.element.querySelectorAll(
            '.fdcp-horizontal-radios__card'
        ).forEach(card => {
            card.addEventListener('click', event => {
                if (!window.matchMedia('(width < 64em)').matches
                    || event.target.closest('input, label')) {
                    return;
                }

                const input = card.querySelector(
                    '.fdcp-horizontal-radios__input[type="radio"]'
                );

                if (!input?.disabled) {
                    input.click();
                }
            });
        });
    }

    bindResponsiveLabelLayout() {
        if (this.labelTexts.length === 0) {
            return;
        }

        this.horizontalLayoutMedia = window.matchMedia('(width >= 48em)');
        this.lastObservedWidth = null;
        this.labelLayoutFrame = null;

        const scheduleUpdate = () => {
            if (this.labelLayoutFrame !== null) {
                cancelAnimationFrame(this.labelLayoutFrame);
            }

            this.labelLayoutFrame = requestAnimationFrame(() => {
                this.labelLayoutFrame = null;
                this.updateResponsiveLabelLayout();
            });
        };

        this.labelLayoutObserver = new ResizeObserver(entries => {
            const width = entries[0]?.contentRect.width;
            if (width === this.lastObservedWidth) {
                return;
            }

            this.lastObservedWidth = width;
            scheduleUpdate();
        });
        this.labelLayoutObserver.observe(this.element);
        this.horizontalLayoutMedia.addEventListener('change', scheduleUpdate);

        document.fonts?.ready.then(scheduleUpdate);
        scheduleUpdate();
    }

    updateResponsiveLabelLayout() {
        const stackedClass = 'fdcp-horizontal-radios--stacked-labels';
        this.element.classList.remove(stackedClass);

        if (!this.horizontalLayoutMedia.matches) {
            return;
        }

        const anyLabelWrapped = this.labelTexts.some(text => {
            const label = text.closest('.fdcp-horizontal-radios__label');
            if (!label) {
                return false;
            }

            const labelTop = label.getBoundingClientRect().top;
            const textTop = text.getBoundingClientRect().top;
            return textTop - labelTop > 1;
        });

        this.element.classList.toggle(stackedClass, anyLabelWrapped);
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
}

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('[data-fdcp-horizontal-radios]').forEach(element => {
        new FDCPHorizontalRadios(element);
    });
});
