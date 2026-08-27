class FDCPTable {
    constructor(element) {
        this.table = element;
        this.observer = new MutationObserver((mutations) => {
            const hasRowSpans = mutations.some(m =>
                Array.from(m.addedNodes).some(n =>
                    n.nodeType === 1 && n.matches?.('span[slot]')
                )
            );
            if (hasRowSpans) {
                this.dispatchRowsRendered();
            }
        });
        this.observer.observe(this.table, { childList: true, subtree: true });

        document.addEventListener('fdcp-table:rows-rendered', (e) => {
            if (e.detail.table === this.table) {
                this.bindAllSlottedChildren();
            }
        });
    }

    dispatchRowsRendered() {
        document.dispatchEvent(new CustomEvent('fdcp-table:rows-rendered', {
            detail: { table: this.table }
        }));
    }

    bindAllSlottedChildren() {
        const wrappers = this.table.querySelectorAll('[slot^="cell-"]');

        wrappers.forEach(wrapper => {
            const firstChild = wrapper.querySelector('*');
            if (!firstChild) return;

            const row = firstChild.row;
            const column = firstChild.column;
            const rowIndex = firstChild.rowIndex;
            const value = firstChild.value;

            wrapper.querySelectorAll('*').forEach(el => {
                this.applyDataBindings(el, row);
                el.row = row;
                el.column = column;
                el.rowIndex = rowIndex;
                el.value = value;
            });
        });
    }

    applyDataBindings(el, row) {
        Array.from(el.attributes)
            .filter(attr => attr.name.startsWith('data-bind'))
            .forEach(attr => {
                let prop, value;
                if (attr.name === 'data-bind-template') {
                    prop = 'textContent';
                    value = attr.value.replace(/\{(\w+)\}/g, (_, field) => String(row[field] ?? ''));
                } else if (attr.name === 'data-bind') {
                    prop = 'textContent';
                    value = row[attr.value];
                } else if (attr.name.startsWith('data-bind-template-')) {
                    prop = attr.name.replace('data-bind-template-', '');
                    value = attr.value.replace(/\{(\w+)\}/g, (_, field) => String(row[field] ?? ''));
                } else {
                    prop = attr.name.replace('data-bind-', '');
                    value = row[attr.value];
                }

                if (prop in el) {
                    el[prop] = value;
                } else {
                    el.setAttribute(prop, String(value ?? ''));
                }
            });
    }

    destroy() {
        this.observer?.disconnect();
    }
}

document.querySelectorAll('gcds-table').forEach(el => new FDCPTable(el));