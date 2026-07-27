class FDCPSearchableSelect {
    constructor(element) {
        this.element = element;
        this.trigger = element.querySelector('[data-fdcp-searchable-select-trigger]');
        this.panel = element.querySelector('[data-fdcp-searchable-select-panel]');
        this.search = element.querySelector('[data-fdcp-searchable-select-search]');
        this.inputs = Array.from(element.querySelectorAll('.fdcp-searchable-select__input'));
        this.options = Array.from(element.querySelectorAll('[data-fdcp-searchable-select-option]'));
        this.singleInput = element.querySelector('[data-fdcp-searchable-select-single-input]');
        this.groups = Array.from(element.querySelectorAll('[data-fdcp-searchable-select-group]'));
        this.noResults = element.querySelector('[data-fdcp-searchable-select-no-results]');
        this.status = element.querySelector('[data-fdcp-searchable-select-status]');
        this.selectedText = element.querySelector('[data-fdcp-searchable-select-selected-text]');
        this.error = element.querySelector('[data-fdcp-searchable-select-error]');
        this.selectionMode = element.getAttribute('data-selection-mode') || 'single';
        this.defaultValue = element.getAttribute('data-default-value') || '';
        this.multipleSelectedText = element.getAttribute('data-multiple-selected-text') || 'selected';
        this.oneResultText = element.getAttribute('data-one-result-text') || '1 result available';
        this.multipleResultsText = element.getAttribute('data-multiple-results-text') || '{0} results available';
        this.isRequired = element.getAttribute('data-required') === 'true';
        this.requiredMessage = element.getAttribute('data-required-message') || 'This field is required.';
        this.requiredSummaryMessage = element.getAttribute('data-required-summary-message') || this.requiredMessage;
        this.pointerDownStartedInside = false;
        this.hasValidationError = false;
        this.form = element.closest('form');

        this.bindEvents();
        this.updateSelectionSummary();
        this.updateSingleOptionTabIndexes();
        this.setActiveSingleOption(this.getSelectedSingleOption());
        this.updateSearchStatus(this.getVisibleEnabledOptions().length);
    }

    bindEvents() {
        if (this.trigger) {
            this.trigger.addEventListener('click', () => this.toggle());
            this.trigger.addEventListener('keydown', event => {
                if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
                    event.preventDefault();
                    this.open();

                    if (this.selectionMode === 'single') {
                        this.element.classList.remove('fdcp-searchable-select--pointer-active');
                        this.element.classList.add('fdcp-searchable-select--keyboard-active');
                        this.selectAdjacentSingleOption(event.key === 'ArrowDown' ? 1 : -1, this.getSelectedSingleOption(), { focusOption: false });
                    }
                }
            });
        }

        if (this.search) {
            this.search.addEventListener('input', () => this.filterOptions());
            this.search.addEventListener('keydown', event => {
                if (event.key === 'Escape') {
                    event.preventDefault();
                    this.close(true);
                } else if (event.key === 'ArrowDown') {
                    event.preventDefault();
                    if (this.selectionMode === 'single') {
                        this.element.classList.remove('fdcp-searchable-select--pointer-active');
                        this.element.classList.add('fdcp-searchable-select--keyboard-active');
                        this.selectAdjacentSingleOption(1, this.getSelectedSingleOption(), { focusOption: false });
                    } else {
                        this.focusFirstVisibleOption();
                    }
                } else if (event.key === 'ArrowUp' && this.selectionMode === 'single') {
                    event.preventDefault();
                    this.element.classList.remove('fdcp-searchable-select--pointer-active');
                    this.element.classList.add('fdcp-searchable-select--keyboard-active');
                    this.selectAdjacentSingleOption(-1, this.getSelectedSingleOption(), { focusOption: false });
                } else if (event.key === 'Enter' && this.selectionMode === 'single') {
                    event.preventDefault();
                    this.selectActiveSingleOption();
                } else if (event.key === 'Tab' && this.selectionMode === 'single') {
                    event.preventDefault();
                    this.selectActiveSingleOption({ closeAfterSelect: false });
                    this.close(true);
                }
            });
        }

        this.inputs.forEach(input => {
            input.addEventListener('change', () => {
                this.updateSelectionSummary();
                this.validateRequired();

                if (this.selectionMode === 'single') {
                    this.close();
                }
            });
        });

        this.options.forEach(option => {
            option.addEventListener('mouseenter', () => {
                if (this.selectionMode === 'single') {
                    this.element.classList.remove('fdcp-searchable-select--keyboard-active');
                    this.element.classList.add('fdcp-searchable-select--pointer-active');

                    if (!option.hidden && !this.isOptionDisabled(option)) {
                        this.setActiveSingleOption(option);
                    }
                }
            });

            option.addEventListener('click', () => {
                if (this.selectionMode === 'single') {
                    this.selectSingleOption(option);
                }
            });

            option.addEventListener('keydown', event => {
                if (this.selectionMode !== 'single') {
                    return;
                }

                this.handleSingleOptionKeydown(event, option);
            });
        });

        this.element.querySelectorAll('[data-fdcp-searchable-select-clear]').forEach(button => {
            button.addEventListener('click', event => {
                event.preventDefault();
                this.clearSelection();
            });
        });

        if (this.form) {
            this.form.addEventListener('submit', event => {
                const shouldFocus = !event.defaultPrevented;

                if (!this.validateRequired({ showError: true, focus: shouldFocus })) {
                    event.preventDefault();
                }
            });
        }

        document.addEventListener('pointerdown', event => {
            this.pointerDownStartedInside = this.element.contains(event.target);
        });

        document.addEventListener('click', event => {
            if (!this.element.contains(event.target)) {
                this.close();
            }

            window.setTimeout(() => {
                this.pointerDownStartedInside = false;
            }, 0);
        });

        this.element.addEventListener('focusout', () => {
            window.setTimeout(() => {
                if (this.pointerDownStartedInside) {
                    return;
                }

                if (!this.element.contains(document.activeElement)) {
                    this.validateRequired({ showError: true });
                    this.close();
                }
            }, 0);
        });
    }

    toggle() {
        if (this.panel && this.panel.hidden) {
            this.open();
        } else {
            this.close();
        }
    }

    open() {
        if (!this.panel || !this.trigger) {
            return;
        }

        this.panel.hidden = false;
        this.trigger.setAttribute('aria-expanded', 'true');
        this.setComboboxExpanded(true);
        this.updatePanelPlacement();

        if (this.search) {
            this.search.focus();
        }
    }

    close(returnFocus = false) {
        if (!this.panel || !this.trigger) {
            return;
        }

        this.panel.hidden = true;
        this.trigger.setAttribute('aria-expanded', 'false');
        this.setComboboxExpanded(false);
        this.element.classList.remove('fdcp-searchable-select--open-above');
        this.element.classList.remove('fdcp-searchable-select--pointer-active');
        this.element.classList.remove('fdcp-searchable-select--keyboard-active');
        this.element.style.removeProperty('--fdcp-searchable-select-trigger-offset');
        this.clearSearch();

        if (this.selectionMode === 'single') {
            this.setActiveSingleOption(this.getSelectedSingleOption());
        }

        if (returnFocus) {
            this.trigger.focus();
        }
    }

    clearSearch() {
        if (!this.search || this.search.value === '') {
            return;
        }

        this.search.value = '';
        this.filterOptions();
    }

    filterOptions() {
        const term = (this.search?.value || '').trim().toLowerCase();
        let visibleCount = 0;

        this.options.forEach(option => {
            const text = (option.getAttribute('data-option-text') || '').toLowerCase();
            const isVisible = text.includes(term);

            option.hidden = !isVisible;
            if (!isVisible) {
                option.setAttribute('tabindex', '-1');
            }

            if (isVisible) {
                visibleCount += 1;
            }
        });

        this.groups.forEach(group => {
            const hasVisibleOption = Array.from(group.querySelectorAll('[data-fdcp-searchable-select-option]'))
                .some(option => !option.hidden);

            group.hidden = !hasVisibleOption;
        });

        if (this.noResults) {
            this.noResults.hidden = visibleCount > 0;
        }

        this.updateSingleOptionTabIndexes();

        if (this.selectionMode === 'single') {
            const selectedOption = this.getSelectedSingleOption();

            if (selectedOption && !selectedOption.hidden && !this.isOptionDisabled(selectedOption)) {
                this.element.classList.remove('fdcp-searchable-select--pointer-active');
                this.element.classList.add('fdcp-searchable-select--keyboard-active');
                this.setActiveSingleOption(selectedOption);
            }
        }

        this.updateSearchStatus(visibleCount);
        this.updatePanelPlacement();
    }

    updatePanelPlacement() {
        if (!this.panel || !this.trigger || this.panel.hidden) {
            return;
        }

        this.element.classList.remove('fdcp-searchable-select--open-above');
        this.element.style.setProperty('--fdcp-searchable-select-trigger-offset', `${this.trigger.offsetTop}px`);

        const triggerRect = this.trigger.getBoundingClientRect();
        const panelHeight = this.panel.getBoundingClientRect().height;
        const viewportHeight = window.innerHeight || document.documentElement.clientHeight;
        const spaceBelow = viewportHeight - triggerRect.bottom;
        const spaceAbove = triggerRect.top;
        const shouldOpenAbove = panelHeight > spaceBelow && spaceAbove > spaceBelow;

        this.element.classList.toggle('fdcp-searchable-select--open-above', shouldOpenAbove);
    }

    clearSelection() {
        if (this.selectionMode === 'single') {
            if (this.singleInput) {
                this.singleInput.value = '';
            }

            this.options.forEach(option => {
                option.classList.remove('is-selected');
                option.classList.remove('is-active');
                option.setAttribute('aria-selected', 'false');
                option.setAttribute('tabindex', '-1');
            });
            this.setActiveSingleOption(null);
        } else {
            this.inputs.forEach(input => {
                input.checked = false;
            });
        }

        this.updateSelectionSummary();
        this.validateRequired();
    }

    selectSingleOption(option, closeAfterSelect = true) {
        if (!option || this.isOptionDisabled(option)) {
            return;
        }

        const value = option.getAttribute('data-option-value') || '';

        if (this.singleInput) {
            this.singleInput.value = value;
        }

        this.options.forEach(currentOption => {
            const isSelected = currentOption === option;

            currentOption.classList.toggle('is-selected', isSelected);
            currentOption.setAttribute('aria-selected', isSelected.toString());
            currentOption.setAttribute('tabindex', '-1');
        });

        this.setActiveSingleOption(option);
        this.updateSelectionSummary();
        this.validateRequired();

        if (closeAfterSelect) {
            this.close(true);
        }
    }

    handleSingleOptionKeydown(event, option) {
        if (event.key === 'ArrowDown') {
            event.preventDefault();
            this.element.classList.remove('fdcp-searchable-select--pointer-active');
            this.element.classList.add('fdcp-searchable-select--keyboard-active');
            this.selectAdjacentSingleOption(1, option);
        } else if (event.key === 'ArrowUp') {
            event.preventDefault();
            this.element.classList.remove('fdcp-searchable-select--pointer-active');
            this.element.classList.add('fdcp-searchable-select--keyboard-active');
            this.selectAdjacentSingleOption(-1, option);
        } else if (event.key === 'Enter' || event.key === ' ') {
            event.preventDefault();
            this.selectSingleOption(option);
        } else if (event.key === 'Tab') {
            event.preventDefault();
            this.selectSingleOption(option, false);
            this.close(true);
        } else if (event.key === 'Escape') {
            event.preventDefault();
            this.close(true);
        }
    }

    focusFirstVisibleOption() {
        const option = this.getVisibleEnabledOptions()[0];

        if (option) {
            option.focus();
        }
    }

    focusAdjacentOption(currentOption, direction, { focusOption = true } = {}) {
        const visibleOptions = this.getVisibleEnabledOptions();
        const currentIndex = visibleOptions.indexOf(currentOption);
        const nextIndex = currentIndex + direction;
        const nextOption = visibleOptions[nextIndex];

        if (nextOption && focusOption) {
            nextOption.focus();
        }

        return nextOption;
    }

    selectAdjacentSingleOption(direction, currentOption = this.getSelectedSingleOption(), { focusOption = true } = {}) {
        const visibleOptions = this.getVisibleEnabledOptions();

        if (visibleOptions.length === 0) {
            this.setActiveSingleOption(null);
            return;
        }

        let nextOption;

        if (!currentOption || currentOption.hidden || this.isOptionDisabled(currentOption)) {
            nextOption = direction > 0 ? visibleOptions[0] : visibleOptions[visibleOptions.length - 1];
        } else {
            nextOption = this.focusAdjacentOption(currentOption, direction, { focusOption });
        }

        if (nextOption) {
            this.selectSingleOption(nextOption, false);
            nextOption.scrollIntoView({ block: 'nearest', inline: 'nearest' });

            if (focusOption) {
                nextOption.focus();
            }
        }
    }

    selectActiveSingleOption({ closeAfterSelect = true } = {}) {
        const option = this.getActiveSingleOption()
            || this.getSelectedSingleOption()
            || this.getVisibleEnabledOptions()[0];

        if (option && !option.hidden && !this.isOptionDisabled(option)) {
            this.selectSingleOption(option, closeAfterSelect);
        }
    }

    updateSingleOptionTabIndexes() {
        if (this.selectionMode !== 'single') {
            return;
        }

        const selectedOption = this.options.find(option => option.getAttribute('aria-selected') === 'true' && !option.hidden && !this.isOptionDisabled(option));
        const firstOption = this.getVisibleEnabledOptions()[0];
        const activeOption = this.getActiveSingleOption();

        this.options.forEach(option => {
            option.setAttribute('tabindex', '-1');
        });

        if (!activeOption || activeOption.hidden || this.isOptionDisabled(activeOption)) {
            this.setActiveSingleOption(selectedOption || firstOption || null);
        }
    }

    getVisibleEnabledOptions() {
        return this.options.filter(option => !option.hidden && !this.isOptionDisabled(option));
    }

    isOptionDisabled(option) {
        return option.getAttribute('aria-disabled') === 'true';
    }

    getActiveSingleOption() {
        const activeOptionId = this.search?.getAttribute('aria-activedescendant') || '';

        if (!activeOptionId) {
            return null;
        }

        return this.options.find(option => option.id === activeOptionId) || null;
    }

    setActiveSingleOption(option) {
        if (this.selectionMode !== 'single') {
            return;
        }

        const optionId = option?.id || '';

        this.options.forEach(currentOption => {
            currentOption.classList.toggle('is-active', currentOption === option);
        });

        if (!this.search) {
            return;
        }

        if (optionId) {
            this.search.setAttribute('aria-activedescendant', optionId);
        } else {
            this.search.removeAttribute('aria-activedescendant');
        }
    }

    setComboboxExpanded(isExpanded) {
        if (this.selectionMode !== 'single' || !this.search) {
            return;
        }

        this.search.setAttribute('aria-expanded', isExpanded.toString());
    }

    getSelectedSingleOption() {
        const selectedValue = this.singleInput?.value || '';

        if (!selectedValue) {
            return null;
        }

        return this.options.find(option => option.getAttribute('data-option-value') === selectedValue) || null;
    }

    getSelectedOptions() {
        if (this.selectionMode === 'single') {
            const selectedValue = this.singleInput?.value || '';

            if (!selectedValue) {
                return [];
            }

            const selectedOption = this.options.find(option => option.getAttribute('data-option-value') === selectedValue);

            return [{
                value: selectedValue,
                label: selectedOption?.getAttribute('data-option-label') || selectedValue
            }];
        }

        return this.inputs
            .filter(input => input.checked)
            .map(input => ({
                value: input.value,
                label: input.getAttribute('data-option-label') || input.value
            }));
    }

    updateSelectionSummary() {
        const selectedOptions = this.getSelectedOptions();
        const labels = selectedOptions.map(option => option.label);
        const summary = selectedOptions.length === 0
            ? this.defaultValue
            : this.selectionMode === 'multiple'
                ? `${selectedOptions.length} ${this.multipleSelectedText}`
                : labels.join(', ');

        if (this.selectedText) {
            this.selectedText.textContent = summary;
        }

        this.element.querySelectorAll('[data-fdcp-searchable-select-selected-count]').forEach(element => {
            element.textContent = selectedOptions.length.toString();
        });

        this.element.dispatchEvent(new CustomEvent('fdcp-searchable-select:change', {
            bubbles: true,
            detail: {
                selectedOptions
            }
        }));
    }

    validateRequired({ showError = false, focus = false } = {}) {
        if (!this.isRequired || this.trigger?.disabled) {
            return true;
        }

        const isValid = this.getSelectedOptions().length > 0;
        const shouldShowError = !isValid && (showError || this.hasValidationError);

        this.setValidationState(isValid, shouldShowError);

        if (!isValid && focus) {
            this.trigger?.focus();
        }

        return isValid;
    }

    setValidationState(isValid, shouldShowError) {
        this.hasValidationError = shouldShowError;

        if (shouldShowError) {
            this.trigger?.setAttribute('aria-invalid', 'true');
            this.trigger?.dispatchEvent(new CustomEvent('gcdsError', {
                bubbles: true,
                composed: true,
                detail: {
                    message: this.requiredSummaryMessage
                }
            }));
        } else {
            this.trigger?.removeAttribute('aria-invalid');

            if (isValid) {
                this.trigger?.dispatchEvent(new CustomEvent('gcdsValid', {
                    bubbles: true,
                    composed: true
                }));
            }
        }

        if (!this.error) {
            return;
        }

        if (shouldShowError) {
            this.error.textContent = this.requiredMessage;
            this.error.hidden = false;
        } else if (isValid) {
            this.error.hidden = true;
        }
    }

    updateSearchStatus(visibleCount = this.getVisibleEnabledOptions().length) {
        if (!this.status) {
            return;
        }

        if (visibleCount === 0) {
            this.status.textContent = this.noResults?.textContent || 'No results found';
        } else if (visibleCount === 1) {
            this.status.textContent = this.oneResultText;
        } else {
            this.status.textContent = this.multipleResultsText.replace('{0}', visibleCount.toString());
        }
    }
}

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('[data-fdcp-searchable-select]').forEach(element => {
        if (!element.FDCPSearchableSelectInstance) {
            element.FDCPSearchableSelectInstance = new FDCPSearchableSelect(element);
        }
    });
});

window.FDCPSearchableSelect = FDCPSearchableSelect;
