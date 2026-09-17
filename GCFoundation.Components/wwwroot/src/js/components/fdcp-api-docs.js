// Progressive enhancement: all documentation and anchor links work without JavaScript.
function initializeFDCPApiDocs() {
    document.querySelectorAll('[data-fdcp-api-docs="true"]').forEach((root) => {
        if (root.dataset.navigationReady === 'true') return;
        root.dataset.navigationReady = 'true';
        const links = Array.from(root.querySelectorAll('.api-docs__nav a[href^="#"]'));
        const sections = Array.from(root.querySelectorAll('[data-api-section]'));
        let scheduled = false;

        function updateCurrentSection() {
            scheduled = false;
            let current = sections[0];
            sections.forEach((section) => {
                if (section.getBoundingClientRect().top <= 100) current = section;
            });
            links.forEach((link) => {
                if (current && link.getAttribute('href') === '#' + current.id) {
                    link.setAttribute('aria-current', 'location');
                } else {
                    link.removeAttribute('aria-current');
                }
            });
        }
        function scheduleUpdate() {
            if (!scheduled) {
                scheduled = true;
                window.requestAnimationFrame(updateCurrentSection);
            }
        }
        root.querySelectorAll('pre').forEach((block) => {
            if (!block.hasAttribute('tabindex')) block.setAttribute('tabindex', '0');
        });
        window.addEventListener('scroll', scheduleUpdate, { passive: true });
        window.addEventListener('resize', scheduleUpdate);
        window.addEventListener('hashchange', scheduleUpdate);
        root.addEventListener('toggle', scheduleUpdate, true);
        updateCurrentSection();
    });
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeFDCPApiDocs);
} else {
    initializeFDCPApiDocs();
}
document.addEventListener('fdcp-tabs:loaded', initializeFDCPApiDocs);
