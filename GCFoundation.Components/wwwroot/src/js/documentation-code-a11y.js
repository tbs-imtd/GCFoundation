(() => {
    'use strict';

    /**
     * Prism sets tabindex="0" on <pre> parents for keyboard scroll access.
     * Add role and name so the focusable pre is a valid widget (WCAG 4.1.2).
     */
    function enhanceCodeBlocks() {
        document.querySelectorAll('.documentation-content pre[tabindex="0"]').forEach((pre) => {
            if (!pre.querySelector('code[class*="language-"]')) {
                return;
            }

            if (!pre.hasAttribute('role')) {
                pre.setAttribute('role', 'region');
            }

            if (pre.hasAttribute('aria-label') || pre.hasAttribute('aria-labelledby')) {
                return;
            }

            const sectionHeading = pre.closest('.documentation-content')?.querySelector('h2, h3');
            const sectionTitle = sectionHeading?.textContent?.trim();
            const label = sectionTitle ? `Code sample: ${sectionTitle}` : 'Code sample';
            pre.setAttribute('aria-label', label);
        });
    }

    function scheduleEnhance() {
        enhanceCodeBlocks();
        window.setTimeout(enhanceCodeBlocks, 0);
        window.setTimeout(enhanceCodeBlocks, 150);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', scheduleEnhance);
    } else {
        scheduleEnhance();
    }

    const documentationRoot = document.querySelector('.documentation-content');
    if (documentationRoot) {
        const observer = new MutationObserver(() => scheduleEnhance());
        observer.observe(documentationRoot, {
            childList: true,
            subtree: true,
            attributes: true,
            attributeFilter: ['tabindex', 'class']
        });
    }
})();
