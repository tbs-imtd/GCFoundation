class FDCPTabs {
    constructor(element) {
        this.element = element;
        this.tabs = Array.from(element.querySelectorAll(':scope > .fdcp-tabs__tablist > [role="tab"]'));
        this.panels = Array.from(element.querySelectorAll(':scope > [role="tabpanel"]'));

        this.bindEvents();
        this.loadSelectedTab();
    }

    bindEvents() {
        this.tabs.forEach((tab, index) => {
            tab.addEventListener('click', () => this.selectTab(index, true));
            tab.addEventListener('keydown', (event) => this.handleKeydown(event, index));
        });
    }

    handleKeydown(event, index) {
        const lastIndex = this.tabs.length - 1;
        let nextIndex = index;

        switch (event.key) {
            case 'ArrowLeft':
                nextIndex = index === 0 ? lastIndex : index - 1;
                break;
            case 'ArrowRight':
                nextIndex = index === lastIndex ? 0 : index + 1;
                break;
            case 'Home':
                nextIndex = 0;
                break;
            case 'End':
                nextIndex = lastIndex;
                break;
            case 'Enter':
            case ' ':
                event.preventDefault();
                this.selectTab(index, true);
                return;
            default:
                return;
        }

        event.preventDefault();
        this.focusTab(nextIndex);
    }

    focusTab(index) {
        const focusedTab = this.tabs[index];

        if (!focusedTab) {
            return;
        }

        this.tabs.forEach((tab, tabIndex) => {
            tab.setAttribute('tabindex', tabIndex === index ? '0' : '-1');
        });
        focusedTab.focus();
    }

    selectTab(index, focusTab) {
        const selectedTab = this.tabs[index];

        if (!selectedTab) {
            return;
        }

        const selectionChanged = selectedTab.getAttribute('aria-selected') !== 'true';

        this.tabs.forEach((tab, tabIndex) => {
            const isSelected = tabIndex === index;
            tab.setAttribute('aria-selected', isSelected ? 'true' : 'false');
            tab.setAttribute('tabindex', isSelected ? '0' : '-1');
        });

        this.panels.forEach((panel) => {
            const isSelected = panel.id === selectedTab.getAttribute('aria-controls');
            panel.hidden = !isSelected;
        });

        if (focusTab) {
            selectedTab.focus();
        }

        if (selectionChanged) {
            this.dispatchChangeEvent(index, selectedTab);
        }

        this.loadTabPanel(selectedTab);
    }

    dispatchChangeEvent(index, tab) {
        const panel = this.getPanelForTab(tab);

        this.element.dispatchEvent(new CustomEvent('fdcp-tabs:change', {
            bubbles: true,
            detail: {
                index,
                tab,
                panel,
                loadUrl: tab.dataset.loadUrl || ''
            }
        }));
    }

    getPanelForTab(tab) {
        const panelId = tab.getAttribute('aria-controls');
        return this.panels.find((panel) => panel.id === panelId) || null;
    }

    loadSelectedTab() {
        const selectedIndex = this.tabs.findIndex((tab) => tab.getAttribute('aria-selected') === 'true');

        if (selectedIndex >= 0) {
            this.loadTabPanel(this.tabs[selectedIndex]);
        }
    }

    async loadTabPanel(tab) {
        const panel = this.getPanelForTab(tab);
        const url = tab.dataset.loadUrl;

        if (!panel || !url || panel.dataset.loaded === 'true' || panel.dataset.loading === 'true') {
            return;
        }

        panel.dataset.loading = 'true';
        panel.setAttribute('aria-busy', 'true');
        this.setStatus(panel, this.element.dataset.loadingText || 'Loading...');

        try {
            const response = await fetch(url, {
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error(`Request failed: ${response.status}`);
            }

            panel.innerHTML = await response.text();
            panel.dataset.loaded = 'true';
            this.element.dispatchEvent(new CustomEvent('fdcp-tabs:loaded', {
                bubbles: true,
                detail: {
                    tab,
                    panel,
                    loadUrl: url
                }
            }));
        } catch (error) {
            this.setStatus(
                panel,
                this.element.dataset.loadErrorText || 'Unable to load this tab. Please try again.'
            );
            this.element.dispatchEvent(new CustomEvent('fdcp-tabs:error', {
                bubbles: true,
                detail: {
                    tab,
                    panel,
                    loadUrl: url,
                    error
                }
            }));
        } finally {
            delete panel.dataset.loading;
            panel.removeAttribute('aria-busy');
        }
    }

    setStatus(panel, message) {
        const status = document.createElement('p');
        status.className = 'fdcp-tabs__status';
        status.textContent = message;
        panel.replaceChildren(status);
    }
}

function initializeFDCPTabs() {
    document.querySelectorAll('[data-fdcp-tabs="true"]').forEach((tabs) => {
        if (tabs.dataset.fdcpTabsInitialized === 'true') {
            return;
        }

        tabs.dataset.fdcpTabsInitialized = 'true';
        new FDCPTabs(tabs);
    });
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeFDCPTabs);
} else {
    initializeFDCPTabs();
}
