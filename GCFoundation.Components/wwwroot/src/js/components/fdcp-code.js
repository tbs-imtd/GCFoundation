// Progressive enhancement: code stays readable and selectable without JavaScript or Prism.
(function () {
    // Larger sources stay plain text to keep the page responsive.
    const MAX_HIGHLIGHT_LENGTH = 50000;
    const STATUS_RESET_DELAY = 5000;
    const groupSelections = new Map();

    if (window.Prism && window.Prism.hooks) {
        // Prism highlights language-* blocks on load; the component highlights its own blocks on demand.
        window.Prism.hooks.add('before-all-elements-highlight', (env) => {
            env.elements = env.elements.filter((element) => !element.closest('[data-fdcp-code]'));
        });
    }

    function highlight(code) {
        if (code.dataset.fdcpCodeHighlight) return;
        const panel = code.closest('[data-fdcp-code-panel]');
        const language = panel ? panel.dataset.fdcpCodeLanguage : '';
        const prism = window.Prism;
        const grammar = prism && prism.languages && language !== 'plaintext' ? prism.languages[language] : null;
        const source = code.textContent;

        if (!grammar || typeof grammar !== 'object') {
            code.dataset.fdcpCodeHighlight = 'none';
            return;
        }
        if (source.length > MAX_HIGHLIGHT_LENGTH) {
            code.dataset.fdcpCodeHighlight = 'skipped';
            return;
        }
        try {
            code.innerHTML = prism.highlight(source, grammar, language);
            code.dataset.fdcpCodeHighlight = 'done';
        } catch (error) {
            code.textContent = source;
            code.dataset.fdcpCodeHighlight = 'failed';
        }
    }

    const observer = 'IntersectionObserver' in window
        ? new IntersectionObserver((entries) => {
            entries.forEach((entry) => {
                if (!entry.isIntersecting) return;
                observer.unobserve(entry.target);
                highlight(entry.target);
            });
        }, { rootMargin: '200px 0px' })
        : null;

    class FDCPCode {
        constructor(root) {
            this.root = root;
            this.panels = Array.from(root.querySelectorAll('[data-fdcp-code-panel]'));
            this.controls = root.querySelector('[data-fdcp-code-controls]');
            this.select = root.querySelector('[data-fdcp-code-select]');
            this.copyButton = root.querySelector('[data-fdcp-code-copy]');
            this.status = root.querySelector('[data-fdcp-code-status]');
            this.syncGroup = root.dataset.fdcpCodeSyncGroup || '';
            this.statusTimer = null;
            this.sources = new Map(this.panels.map((panel) => {
                const code = panel.querySelector('[data-fdcp-code-source]');
                return [panel, code ? code.textContent : ''];
            }));

            this.bindEvents();
            this.enhance();
        }

        bindEvents() {
            if (this.select) {
                this.select.addEventListener('change', () => this.onLanguageChange(this.select.value));
            }
            if (this.copyButton) {
                this.copyButton.addEventListener('click', () => this.copy());
            }
        }

        enhance() {
            const active = this.activePanel();
            const groupLanguage = this.syncGroup ? groupSelections.get(this.syncGroup) : null;
            if (!(groupLanguage && this.activate(groupLanguage)) && active) {
                this.activate(active.dataset.fdcpCodeLanguage);
            }
            if (this.controls) this.controls.hidden = false;
            this.root.classList.add('fdcp-code--enhanced');

            this.panels.forEach((panel) => {
                const code = panel.querySelector('[data-fdcp-code-source]');
                if (!code) return;
                if (observer) observer.observe(code);
                else highlight(code);
            });
        }

        activePanel() {
            return this.panels.find((panel) => panel.dataset.fdcpCodeActive === 'true') || this.panels[0];
        }

        activate(language) {
            const target = this.panels.find((panel) => panel.dataset.fdcpCodeLanguage === language);
            if (!target) return false;

            this.panels.forEach((panel) => {
                const isActive = panel === target;
                panel.hidden = this.panels.length > 1 && !isActive;
                if (isActive) panel.dataset.fdcpCodeActive = 'true';
                else delete panel.dataset.fdcpCodeActive;
            });
            if (this.select && this.select.value !== language) this.select.value = language;
            this.setStatus('');
            return true;
        }

        onLanguageChange(language) {
            if (!this.activate(language)) return;
            this.root.dispatchEvent(new CustomEvent('fdcp-code:change', { bubbles: true, detail: { language } }));
            if (!this.syncGroup) return;

            groupSelections.set(this.syncGroup, language);
            document.querySelectorAll('[data-fdcp-code-sync-group]').forEach((other) => {
                if (other === this.root || other.dataset.fdcpCodeSyncGroup !== this.syncGroup || !other.FDCPCodeInstance) return;
                // Examples without this language keep their current valid selection.
                other.FDCPCodeInstance.activate(language);
            });
        }

        copy() {
            const panel = this.activePanel();
            const text = this.sources.get(panel) || '';
            const clipboard = window.isSecureContext && navigator.clipboard;
            const write = clipboard
                ? clipboard.writeText(text)
                : Promise.reject(new Error('Clipboard unavailable'));

            write
                .then(() => this.setStatus(this.root.dataset.fdcpCodeCopied, true))
                .catch(() => {
                    this.selectCode(panel);
                    this.setStatus(this.root.dataset.fdcpCodeCopyFailed);
                });
        }

        selectCode(panel) {
            const code = panel && panel.querySelector('[data-fdcp-code-source]');
            const selection = window.getSelection ? window.getSelection() : null;
            if (!code || !selection) return;
            // Highlighting later would replace the selected nodes and clear the selection.
            highlight(code);
            const range = document.createRange();
            range.selectNodeContents(code);
            selection.removeAllRanges();
            selection.addRange(range);
        }

        setStatus(message, autoClear) {
            if (!this.status) return;
            window.clearTimeout(this.statusTimer);
            this.status.textContent = '';
            if (!message) return;
            // Clearing first lets screen readers announce the same message again on repeated copies.
            window.requestAnimationFrame(() => {
                this.status.textContent = message;
            });
            if (autoClear) {
                this.statusTimer = window.setTimeout(() => {
                    this.status.textContent = '';
                }, STATUS_RESET_DELAY);
            }
        }

        static initialize(container) {
            const scope = container && container.querySelectorAll ? container : document;
            const roots = Array.from(scope.querySelectorAll('[data-fdcp-code]'));
            if (scope.matches && scope.matches('[data-fdcp-code]')) roots.unshift(scope);

            roots.forEach((root) => {
                if (root.dataset.fdcpCodeReady === 'true') return;
                root.dataset.fdcpCodeReady = 'true';
                root.FDCPCodeInstance = new FDCPCode(root);
            });
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => FDCPCode.initialize(document));
    } else {
        FDCPCode.initialize(document);
    }
    document.addEventListener('fdcp-tabs:loaded', (event) => FDCPCode.initialize(event.target));

    window.FDCPCode = FDCPCode;
})();
