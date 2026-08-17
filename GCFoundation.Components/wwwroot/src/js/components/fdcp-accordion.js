class FDCPAccordion {
    constructor(element) {
        this.element = element;
        this.accordionId = element.id;
        this.detailsInGroup = Array.from(element.querySelectorAll('gcds-details'));
        this.topExpandBtn = element.querySelector('[button-id="fdcp-accordion-top-expand-all-button"]');
        this.topCollapseBtn = element.querySelector('[button-id="fdcp-accordion-top-collapse-all-button"]');
        this.bottomExpandBtn = element.querySelector('[button-id="fdcp-accordion-bottom-expand-all-button"]');
        this.bottomCollapseBtn = element.querySelector('[button-id="fdcp-accordion-bottom-collapse-all-button"]');

        this.bindEvents();
    }

    bindEvents() {
        this.element.addEventListener('click', event => {
            if (this.element.dataset.bulkAction === 'true') {
                return;
            }

            const clicked = event.target.closest('gcds-details');
            if (!clicked || !this.detailsInGroup.includes(clicked)) {
                return;
            }

            const isNotAlwaysOpen = this.element.classList.contains('fdcp-accordion-not-always-open');

            requestAnimationFrame(() => {
                if (isNotAlwaysOpen && clicked.hasAttribute('open')) {
                    this.detailsInGroup.forEach(other => {
                        if (other !== clicked && other.hasAttribute('open')) {
                            other.removeAttribute('open');
                        }
                    });
                }
            });
        });

        if (this.topExpandBtn) {
            this.topExpandBtn.addEventListener('click', () => this.openAll());
        }

        if (this.bottomExpandBtn) {
            this.bottomExpandBtn.addEventListener('click', () => this.openAll());
        }

        if (this.topCollapseBtn) {
            this.topCollapseBtn.addEventListener('click', () => this.closeAll());
        }

        if (this.bottomCollapseBtn) {
            this.bottomCollapseBtn.addEventListener('click', () => this.closeAll());
        }
    };
    
    openAll() {
        this.element.dataset.bulkAction = 'true';
        this.detailsInGroup.forEach(details => details.setAttribute('open', ''));
        delete this.element.dataset.bulkAction;
    }

    closeAll() {
        this.element.dataset.bulkAction = 'true';
        this.detailsInGroup.forEach(details => details.removeAttribute('open'));
        delete this.element.dataset.bulkAction;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.fdcp-accordion').forEach(element => {
        if (!element.FDCPAccordionInstance) {
            element.FDCPAccordionInstance = new FDCPAccordion(element);
        }
    });
});

window.FDCPAccordion = FDCPAccordion;