(() => {
    'use strict';

    const WRAPPER_SELECTOR = '.fdcp-table-responsive';
    const SCROLL_HINTS = {
        en: 'Use left and right arrow keys to scroll horizontally.',
        fr: 'Utilisez les flèches gauche et droite pour faire défiler horizontalement.'
    };

    function getLanguage(element) {
        const elementLang = element?.getAttribute('lang');
        if (elementLang) {
            return elementLang.toLowerCase().substring(0, 2);
        }

        const docLang = document.documentElement.lang;
        if (docLang) {
            return docLang.toLowerCase().substring(0, 2);
        }

        return 'en';
    }

    function getScrollHint(element) {
        const lang = getLanguage(element);
        return SCROLL_HINTS[lang] || SCROLL_HINTS.en;
    }

    function getLabelledById(element) {
        if (!element) {
            return null;
        }

        if (element.id) {
            return element.id;
        }

        const innerHeading = element.querySelector?.('h1, h2, h3, h4, h5, h6');
        return innerHeading?.id || null;
    }

    function getPrecedingHeading(wrapper) {
        let node = wrapper.previousElementSibling;

        while (node) {
            if (/^H[1-6]$/i.test(node.tagName)) {
                return node;
            }

            if (node.matches?.('gcds-heading')) {
                return node;
            }

            node = node.previousElementSibling;
        }

        return null;
    }

    function getTableCaptionId(wrapper) {
        const caption = wrapper.querySelector('table > caption[id]');
        return caption?.id || null;
    }

    function getEmbeddedAriaLabel(wrapper) {
        const labelledElement = wrapper.querySelector('[aria-label]');
        const label = labelledElement?.getAttribute('aria-label')?.trim();
        return label || null;
    }

    function ensureScrollHint(wrapper) {
        const hintId = `${wrapper.id || `fdcp-table-scroll-${Math.random().toString(36).slice(2, 9)}`}-scroll-hint`;
        let hint = wrapper.querySelector('.fdcp-table-responsive-scroll-hint');

        if (!hint) {
            hint = document.createElement('span');
            hint.className = 'visibility-sr-only fdcp-table-responsive-scroll-hint';
            hint.id = hintId;
            hint.textContent = getScrollHint(wrapper);
            wrapper.appendChild(hint);
        }

        return hint.id;
    }

    function bindKeyboardScroll(wrapper) {
        if (wrapper.dataset.fdcpScrollBound === 'true') {
            return;
        }

        wrapper.addEventListener('keydown', (event) => {
            const step = 40;
            let handled = false;

            switch (event.key) {
                case 'ArrowLeft':
                    wrapper.scrollLeft -= step;
                    handled = true;
                    break;
                case 'ArrowRight':
                    wrapper.scrollLeft += step;
                    handled = true;
                    break;
                case 'ArrowUp':
                    if (wrapper.scrollHeight > wrapper.clientHeight) {
                        wrapper.scrollTop -= step;
                        handled = true;
                    }
                    break;
                case 'ArrowDown':
                    if (wrapper.scrollHeight > wrapper.clientHeight) {
                        wrapper.scrollTop += step;
                        handled = true;
                    }
                    break;
                default:
                    break;
            }

            if (handled) {
                event.preventDefault();
            }
        });

        wrapper.dataset.fdcpScrollBound = 'true';
    }

    function enhanceResponsiveTable(wrapper) {
        if (!wrapper || wrapper.dataset.fdcpResponsiveEnhanced === 'true') {
            return;
        }

        if (!wrapper.hasAttribute('tabindex')) {
            wrapper.setAttribute('tabindex', '0');
        }

        if (!wrapper.hasAttribute('role')) {
            wrapper.setAttribute('role', 'region');
        }

        const hasName = wrapper.hasAttribute('aria-label') || wrapper.hasAttribute('aria-labelledby');
        if (!hasName) {
            const captionId = getTableCaptionId(wrapper);
            const heading = getPrecedingHeading(wrapper);
            const headingId = getLabelledById(heading);
            const embeddedLabel = getEmbeddedAriaLabel(wrapper);

            if (captionId) {
                wrapper.setAttribute('aria-labelledby', captionId);
            } else if (headingId) {
                wrapper.setAttribute('aria-labelledby', headingId);
            } else if (embeddedLabel) {
                wrapper.setAttribute('aria-label', embeddedLabel);
            } else if (heading?.textContent?.trim()) {
                wrapper.setAttribute('aria-label', `${heading.textContent.trim()} table`);
            } else {
                const lang = getLanguage(wrapper);
                wrapper.setAttribute(
                    'aria-label',
                    lang === 'fr' ? 'Tableau à défilement horizontal' : 'Horizontally scrollable table'
                );
            }
        }

        const hintId = ensureScrollHint(wrapper);
        const describedBy = wrapper.getAttribute('aria-describedby');
        const describedByIds = describedBy ? describedBy.split(/\s+/).filter(Boolean) : [];

        if (!describedByIds.includes(hintId)) {
            describedByIds.push(hintId);
            wrapper.setAttribute('aria-describedby', describedByIds.join(' '));
        }

        bindKeyboardScroll(wrapper);
        wrapper.dataset.fdcpResponsiveEnhanced = 'true';
    }

    function enhanceResponsiveTables() {
        document.querySelectorAll(WRAPPER_SELECTOR).forEach(enhanceResponsiveTable);
    }

    function scheduleEnhance() {
        enhanceResponsiveTables();
        window.setTimeout(enhanceResponsiveTables, 0);
        window.setTimeout(enhanceResponsiveTables, 150);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', scheduleEnhance);
    } else {
        scheduleEnhance();
    }

    const observerRoot = document.body;
    if (observerRoot) {
        const observer = new MutationObserver(() => scheduleEnhance());
        observer.observe(observerRoot, {
            childList: true,
            subtree: true
        });
    }

    window.FDCP = window.FDCP || {};
    window.FDCP.enhanceResponsiveTables = enhanceResponsiveTables;
})();
