(() => {
    'use strict';

    const EDITOR_SELECTOR = '[data-fdcp-rich-text="true"]';
    const TEMPLATE_LABEL_DEFAULT = 'Insert template';

    // Localized error messages
    const ERROR_MESSAGES = {
        en: {
            required: 'This field is required'
        },
        fr: {
            required: 'Ce champ est obligatoire'
        }
    };

    /**
     * Get the current language from the page or element
     */
    function getLanguage(element) {
        // Check element's lang attribute first
        const elementLang = element?.getAttribute('lang');
        if (elementLang) {
            return elementLang.toLowerCase().substring(0, 2);
        }
        
        // Check document lang
        const docLang = document.documentElement.lang;
        if (docLang) {
            return docLang.toLowerCase().substring(0, 2);
        }
        
        // Default to English
        return 'en';
    }

    /**
     * Get localized error message
     */
    function getLocalizedErrorMessage(key, element) {
        const lang = getLanguage(element);
        const messages = ERROR_MESSAGES[lang] || ERROR_MESSAGES['en'];
        return messages[key] || ERROR_MESSAGES['en'][key];
    }

    function initWhenReady() {
        if (typeof window.Quill === 'undefined') {
            if (document.querySelector(EDITOR_SELECTOR)) {
                setTimeout(initWhenReady, 100);
            }
            return;
        }

        initRichText();
    }

    function initRichText() {
        const editors = document.querySelectorAll(EDITOR_SELECTOR);
        if (!editors.length) {
            return;
        }

        editors.forEach(setupEditorInstance);
    }

    function setupEditorInstance(editorContainer) {
        if (!editorContainer || editorContainer.dataset.quillInitialized === 'true') {
            return;
        }

        const inputId = editorContainer.getAttribute('data-for');
        if (!inputId) {
            return;
        }

        const hiddenInput = document.getElementById(inputId);
        if (!hiddenInput) {
            return;
        }

        const toolbarType = editorContainer.getAttribute('data-toolbar') || 'basic';
        const placeholder = editorContainer.getAttribute('data-placeholder') || '';
        const templatesPayload = editorContainer.getAttribute('data-templates');

        const modules = {
            toolbar: getToolbarConfig(toolbarType)
        };

        const quill = new window.Quill(editorContainer, {
            theme: 'snow',
            modules,
            placeholder
        });

        editorContainer.dataset.fdcpInitialValue = hiddenInput.defaultValue;

        applyInitialValue(quill, hiddenInput);
        bindEditorEvents(quill, hiddenInput, editorContainer);
        enhanceAccessibility(editorContainer, hiddenInput);
        appendTemplateMenu(editorContainer, quill, templatesPayload, inputId);
        enhanceToolbarAccessibility(editorContainer, hiddenInput);
        enhanceTooltipAccessibility(editorContainer);
        setupValidation(quill, hiddenInput, editorContainer);
        setupResetHandler(hiddenInput, editorContainer);

        editorContainer.dataset.quillInitialized = 'true';
    }

    function applyInitialValue(quill, hiddenInput) {
        if (!hiddenInput.value) {
            quill.setText('');
            return;
        }

        quill.clipboard.dangerouslyPasteHTML(hiddenInput.value);
    }

    function bindEditorEvents(quill, hiddenInput, editorContainer) {
        if (!quill || !hiddenInput) {
            return;
        }

        const wrapper = editorContainer.closest('.fdcp-rich-text-wrapper');
        const container = editorContainer.closest('.fdcp-rich-text-container');

        quill.on('text-change', () => {
            const html = quill.root.innerHTML;
            const isEmpty = quill.getText().trim().length === 0;
            const newValue = isEmpty ? '' : html;

            if (hiddenInput.value !== newValue) {
                hiddenInput.value = newValue;
                triggerInputEvents(hiddenInput, newValue);
            }

            // Clear error when user starts typing valid content
            if (!isEmpty) {
                removeErrorState(editorContainer, wrapper, container, hiddenInput);
            }
        });

        hiddenInput.addEventListener('invalid', (e) => {
            e.preventDefault(); // Prevent default browser validation UI
            const errorMsg = hiddenInput.getAttribute('data-required-error') || 
                             getLocalizedErrorMessage('required', hiddenInput);
            addErrorState(editorContainer, wrapper, container, hiddenInput, errorMsg);
        });

        hiddenInput.addEventListener('input', () => {
            if (hiddenInput.value && hiddenInput.value.trim().length > 0) {
                removeErrorState(editorContainer, wrapper, container, hiddenInput);
            }
        });
    }

    function getErrorEventTarget(editorContainer, hiddenInput) {
        // Prefer the focusable editor surface so gcds-error-summary can move focus there.
        return editorContainer.querySelector('.ql-editor')
            || document.getElementById(hiddenInput.id)
            || editorContainer;
    }

    function dispatchGcdsError(target, message) {
        if (!target || !message) {
            return;
        }

        target.dispatchEvent(new CustomEvent('gcdsError', {
            bubbles: true,
            composed: true,
            detail: { message }
        }));
    }

    function dispatchGcdsValid(target) {
        if (!target) {
            return;
        }

        target.dispatchEvent(new CustomEvent('gcdsValid', {
            bubbles: true,
            composed: true,
            detail: {}
        }));
    }

    function setupValidation(quill, hiddenInput, editorContainer) {
        const wrapper = editorContainer.closest('.fdcp-rich-text-wrapper');
        const container = editorContainer.closest('.fdcp-rich-text-container');
        const editorArea = editorContainer.querySelector('.ql-editor');

        // Validate on blur (following GCDS pattern)
        if (editorArea) {
            editorArea.addEventListener('blur', () => {
                validateField(quill, hiddenInput, editorContainer, wrapper, container);
            });
        }

        // Handle form submission validation
        const form = hiddenInput.closest('form');
        if (form && !form.hasAttribute('data-rich-text-validation-bound')) {
            form.setAttribute('data-rich-text-validation-bound', 'true');
            form.addEventListener('submit', (e) => {
                // Find all rich text editors in this form and validate them
                const richTextInputs = form.querySelectorAll('input[data-error-id]');
                let hasErrors = false;
                
                richTextInputs.forEach(input => {
                    const editor = form.querySelector(`[data-for="${input.id}"]`);
                    if (editor && editor.dataset.quillInitialized === 'true') {
                        const editorWrapper = editor.closest('.fdcp-rich-text-wrapper');
                        const editorContainer = editor.closest('.fdcp-rich-text-container');
                        const quillInstance = window.Quill?.find(editor);
                        
                        if (quillInstance && !validateField(quillInstance, input, editor, editorWrapper, editorContainer)) {
                            hasErrors = true;
                        }
                    }
                });

                if (hasErrors) {
                    e.preventDefault();
                    // Focus the first error
                    const firstError = form.querySelector('.fdcp-rich-text-wrapper.has-error .ql-editor');
                    if (firstError) {
                        firstError.focus();
                    }
                }
            });
        }
    }

    function setupResetHandler(hiddenInput, editorContainer) {
        const form = hiddenInput.closest('form');
        if (!form || form.hasAttribute('data-rich-text-reset-bound')) {
            return;
        }

        form.setAttribute('data-rich-text-reset-bound', 'true');
        form.addEventListener('reset', () => {
            const richTextEditors = form.querySelectorAll(`${EDITOR_SELECTOR}[data-quill-initialized="true"]`);

            richTextEditors.forEach(editor => {
                const quillInstance = window.Quill?.find(editor);
                if (!quillInstance) {
                    return;
                }

                const wrapper = editor.closest('.fdcp-rich-text-wrapper');
                const container = editor.closest('.fdcp-rich-text-container');
                const input = document.getElementById(editor.getAttribute('data-for'));
                const initialValue = editor.dataset.fdcpInitialValue ?? '';

                if (initialValue) {
                    quillInstance.clipboard.dangerouslyPasteHTML(initialValue);
                } else {
                    quillInstance.setText('');
                }

                if (input) {
                    removeErrorState(editor, wrapper, container, input);
                }
            });
        });
    }

    function validateField(quill, hiddenInput, editorContainer, wrapper, container) {
        const isEmpty = quill.getText().trim().length === 0;
        const isRequired = hiddenInput.hasAttribute('required');

        if (isRequired && isEmpty) {
            // Get localized error message - prefer data attribute, fallback to localized default
            const errorMsg = hiddenInput.getAttribute('data-required-error') || 
                             getLocalizedErrorMessage('required', hiddenInput);
            addErrorState(editorContainer, wrapper, container, hiddenInput, errorMsg);
            return false;
        }

        removeErrorState(editorContainer, wrapper, container, hiddenInput);
        return true;
    }

    function triggerInputEvents(hiddenInput, value) {
        hiddenInput.dispatchEvent(new Event('input', { bubbles: true }));
        hiddenInput.dispatchEvent(new Event('change', { bubbles: true }));
        hiddenInput.dispatchEvent(new CustomEvent('gcdsChange', { detail: value }));
    }

    function addErrorState(editorContainer, wrapper, container, hiddenInput, errorMessage) {
        const editorArea = editorContainer.querySelector('.ql-editor');
        const errorId = hiddenInput.getAttribute('data-error-id') || `${hiddenInput.id}_error`;
        
        // Set aria-invalid on the editor area
        if (editorArea) {
            editorArea.setAttribute('aria-invalid', 'true');
            updateAriaDescribedBy(editorArea, hiddenInput, errorId, true);
        }
        
        editorContainer.setAttribute('aria-invalid', 'true');
        
        if (wrapper) {
            wrapper.classList.add('has-error');
        }

        // Create or update gcds-error-message element
        if (container && errorMessage) {
            let errorElement = container.querySelector(`gcds-error-message[message-id="${errorId}"]`);
            
            if (!errorElement) {
                errorElement = document.createElement('gcds-error-message');
                errorElement.setAttribute('message-id', errorId);
                errorElement.setAttribute('id', errorId);
                
                // Insert before the wrapper (after hint if present)
                const hint = container.querySelector('gcds-hint');
                if (hint && hint.nextSibling) {
                    container.insertBefore(errorElement, hint.nextSibling);
                } else if (wrapper) {
                    container.insertBefore(errorElement, wrapper);
                }
            }
            
            // Set the error message as inner text content (gcds-error-message displays content)
            errorElement.textContent = errorMessage;
        }

        // Notify gcds-error-summary (listen mode) so Bio appears alongside GCDS field errors.
        dispatchGcdsError(getErrorEventTarget(editorContainer, hiddenInput), errorMessage);
    }

    function removeErrorState(editorContainer, wrapper, container, hiddenInput) {
        const editorArea = editorContainer.querySelector('.ql-editor');
        const errorId = hiddenInput.getAttribute('data-error-id') || `${hiddenInput.id}_error`;
        
        if (editorArea) {
            editorArea.removeAttribute('aria-invalid');
            updateAriaDescribedBy(editorArea, hiddenInput, errorId, false);
        }
        
        editorContainer.removeAttribute('aria-invalid');
        
        if (wrapper) {
            wrapper.classList.remove('has-error');
        }

        // Remove the gcds-error-message element
        if (container) {
            const errorElement = container.querySelector(`gcds-error-message[message-id="${errorId}"]`);
            if (errorElement) {
                errorElement.remove();
            }
        }

        dispatchGcdsValid(getErrorEventTarget(editorContainer, hiddenInput));
    }

    function updateAriaDescribedBy(editorArea, hiddenInput, errorId, hasError) {
        const hintId = `${hiddenInput.id}_hint`;
        const describedBy = [];
        
        // Check if hint exists (now has proper id attribute)
        const hintElement = document.getElementById(hintId);
        if (hintElement) {
            describedBy.push(hintId);
        }
        
        if (hasError) {
            describedBy.push(errorId);
        }
        
        if (describedBy.length > 0) {
            editorArea.setAttribute('aria-describedby', describedBy.join(' '));
        } else {
            editorArea.removeAttribute('aria-describedby');
        }
    }

    function enhanceAccessibility(editorContainer, hiddenInput) {
        const editorArea = editorContainer.querySelector('.ql-editor');
        if (!editorArea) {
            return;
        }

        // Set proper ARIA role for the editor area
        editorArea.setAttribute('role', 'textbox');
        editorArea.setAttribute('aria-multiline', 'true');

        // Find and associate with label (it's a span, not a label element)
        const labelId = `${hiddenInput.id}_label`;
        const label = document.getElementById(labelId);
        if (label) {
            editorArea.setAttribute('aria-labelledby', label.id);
            
            // Make label clickable to focus editor (like a real label would)
            label.style.cursor = 'pointer';
            label.addEventListener('click', () => {
                editorArea.focus();
            });
        }

        // Set required attribute if needed
        if (hiddenInput.hasAttribute('required')) {
            editorArea.setAttribute('aria-required', 'true');
        }

        // Build initial describedby
        const describedBy = [];
        const hintId = `${hiddenInput.id}_hint`;
        const hint = document.getElementById(hintId);
        if (hint) {
            describedBy.push(hintId);
        }

        const errorId = `${hiddenInput.id}_error`;
        const error = document.getElementById(errorId);
        if (error) {
            describedBy.push(errorId);
        }

        if (describedBy.length) {
            editorArea.setAttribute('aria-describedby', describedBy.join(' '));
        }
    }

    function appendTemplateMenu(editorContainer, quill, templatesPayload, inputId) {
        if (!templatesPayload) {
            return;
        }

        let templates;
        try {
            templates = JSON.parse(templatesPayload);
        } catch (error) {
            console.error('FDCP Rich Text: invalid template JSON', error);
            return;
        }

        if (!templates || typeof templates !== 'object') {
            return;
        }

        const entries = Object.entries(templates).filter(([_, value]) => typeof value === 'string' && value.trim().length);
        if (!entries.length) {
            return;
        }

        const wrapper = editorContainer.closest('.fdcp-rich-text-wrapper');
        if (!wrapper) {
            return;
        }

        const menu = document.createElement('div');
        menu.className = 'fdcp-rich-text-template-menu';

        const selectId = `${inputId}-template-select`;
        const label = document.createElement('label');
        label.className = 'fdcp-rich-text-template-label';
        label.id = `${inputId}-template-label`;
        label.textContent = TEMPLATE_LABEL_DEFAULT;
        label.setAttribute('for', selectId);

        const select = document.createElement('select');
        select.className = 'fdcp-rich-text-template-select';
        select.id = selectId;
        select.setAttribute('aria-labelledby', label.id);

        const defaultOption = document.createElement('option');
        defaultOption.value = '';
        defaultOption.textContent = TEMPLATE_LABEL_DEFAULT;
        select.appendChild(defaultOption);

        entries.forEach(([name]) => {
            const option = document.createElement('option');
            option.value = name;
            option.textContent = name;
            select.appendChild(option);
        });

        select.addEventListener('change', (event) => {
            const key = event.target.value;
            if (!key || !templates[key]) {
                return;
            }

            const cursorIndex = quill.getSelection()?.index ?? quill.getLength();
            quill.clipboard.dangerouslyPasteHTML(cursorIndex, templates[key]);
            quill.focus();
            event.target.value = '';
        });

        menu.appendChild(label);
        menu.appendChild(select);

        const toolbar = wrapper.querySelector('.ql-toolbar');
        if (toolbar && toolbar.nextSibling) {
            wrapper.insertBefore(menu, toolbar.nextSibling);
        } else {
            wrapper.insertBefore(menu, editorContainer);
        }
    }

    function enhanceToolbarAccessibility(editorContainer, hiddenInput) {
        const wrapper = editorContainer.closest('.fdcp-rich-text-wrapper');
        const toolbar = wrapper?.querySelector('.ql-toolbar');
        if (!toolbar) {
            return;
        }

        const controlLabel = document.getElementById(`${hiddenInput.id}_label`);
        const labelText = controlLabel?.textContent?.trim();
        toolbar.setAttribute('role', 'toolbar');
        if (labelText) {
            toolbar.setAttribute('aria-label', `${labelText} formatting toolbar`);
        }

        toolbar.querySelectorAll('button').forEach(button => {
            const label = getButtonLabel(button);
            if (label && !button.getAttribute('aria-label')) {
                button.setAttribute('aria-label', label);
            }
        });

        toolbar.querySelectorAll('.ql-picker').forEach(picker => {
            const label = getPickerLabel(picker);
            if (!label) {
                return;
            }
            const trigger = picker.querySelector('.ql-picker-label');
            if (trigger && !trigger.getAttribute('aria-label')) {
                trigger.setAttribute('aria-label', label);
                trigger.setAttribute('role', 'button');
            }
        });

        toolbar.querySelectorAll('select').forEach(select => {
            const label = getPickerLabel(select);
            if (label && !select.getAttribute('aria-label')) {
                select.setAttribute('aria-label', label);
            }
        });
    }

    function enhanceTooltipAccessibility(editorContainer) {
        // The tooltip is often appended to the container, but sometimes to the body or elsewhere depending on config.
        // In standard snow theme, it is inside .ql-container, which is the editorContainer (since we init on it).
        // Or strictly speaking, Quill adds .ql-container class to the element we pass.
        // However, the tooltip might be created lazily. Usually it exists but is hidden.
        
        const wrapper = editorContainer.closest('.fdcp-rich-text-wrapper');
        if (!wrapper) return;

        // Locate the tooltip
        const tooltip = wrapper.querySelector('.ql-tooltip');
        if (!tooltip) return;

        // 1. Fix empty link preview
        const preview = tooltip.querySelector('a.ql-preview');
        if (preview) {
            if (!preview.getAttribute('aria-label')) {
                preview.setAttribute('aria-label', 'Current link URL');
            }
            if (!preview.textContent.trim()) {
                // Ideally it shows the URL, but if empty, screen readers need something.
                // Quill updates textContent when a link is selected.
                // If it is strictly empty, we can give it a title or label.
                // But the issue 'Empty link' suggests the link text is empty.
                // The aria-label should suffice for "A link contains no text".
            }
        }

        // 2. Fix missing form label for the input
        const input = tooltip.querySelector('input[type="text"]');
        if (input && !input.getAttribute('aria-label')) {
            // This input is used for Link, Video, Formula.
            // We can set a generic label or try to be specific if we detect mode (harder).
            input.setAttribute('aria-label', 'Enter link URL');
        }

        // 3. Fix ql-action (Save) and ql-remove (Remove)
        const actionBtn = tooltip.querySelector('a.ql-action');
        if (actionBtn) {
            if (!actionBtn.getAttribute('aria-label')) {
                actionBtn.setAttribute('aria-label', 'Save');
            }
            if (!actionBtn.getAttribute('role')) {
                actionBtn.setAttribute('role', 'button');
            }
        }

        const removeBtn = tooltip.querySelector('a.ql-remove');
        if (removeBtn) {
            if (!removeBtn.getAttribute('aria-label')) {
                removeBtn.setAttribute('aria-label', 'Remove');
            }
            if (!removeBtn.getAttribute('role')) {
                removeBtn.setAttribute('role', 'button');
            }
        }
    }

    function getButtonLabel(button) {
        const classList = Array.from(button.classList);
        if (classList.includes('ql-list')) {
            const value = button.getAttribute('value');
            return value === 'ordered' ? 'Numbered list' : 'Bulleted list';
        }
        if (classList.includes('ql-indent')) {
            const value = button.getAttribute('value');
            return value === '+1' ? 'Increase indent' : 'Decrease indent';
        }
        if (classList.includes('ql-script')) {
            const value = button.getAttribute('value');
            return value === 'sub' ? 'Subscript' : 'Superscript';
        }

        const simpleMap = {
            'ql-bold': 'Bold',
            'ql-italic': 'Italic',
            'ql-underline': 'Underline',
            'ql-strike': 'Strikethrough',
            'ql-link': 'Insert link',
            'ql-image': 'Insert image',
            'ql-video': 'Insert video',
            'ql-clean': 'Clear formatting'
        };

        const key = classList.find(cls => simpleMap[cls]);
        return key ? simpleMap[key] : null;
    }

    function getPickerLabel(picker) {
        const classList = Array.from(picker.classList);
        const map = {
            'ql-header': 'Formatting style',
            'ql-size': 'Font size',
            'ql-font': 'Font family',
            'ql-align': 'Text alignment',
            'ql-color': 'Text color',
            'ql-background': 'Background color'
        };
        const key = classList.find(cls => map[cls]);
        return key ? map[key] : null;
    }

    function getToolbarConfig(type) {
        switch (type) {
            case 'full':
                return [
                    [{ header: [2, 3, 4, 5, 6, false] }],
                    ['bold', 'italic', 'underline', 'strike'],
                    [{ list: 'ordered' }, { list: 'bullet' }],
                    [{ script: 'sub' }, { script: 'super' }],
                    [{ indent: '-1' }, { indent: '+1' }],
                    [{ align: [] }],
                    ['link', 'image', 'video'],
                    ['clean']
                ];
            case 'standard':
                return [
                    [{ header: [2, 3, 4, false] }],
                    ['bold', 'italic', 'underline', 'link'],
                    [{ list: 'ordered' }, { list: 'bullet' }],
                    ['clean']
                ];
            case 'basic':
            default:
                return [
                    ['bold', 'italic', 'underline'],
                    [{ list: 'ordered' }, { list: 'bullet' }],
                    ['link', 'clean']
                ];
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initWhenReady);
    } else {
        initWhenReady();
    }

    window.FDCP = window.FDCP || {};
    window.FDCP.initRichText = initRichText;
})();


