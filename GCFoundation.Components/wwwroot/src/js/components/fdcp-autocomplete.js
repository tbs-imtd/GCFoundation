/**
 * In-memory LRU cache for pages of autocomplete results.
 * Memory only by design: results may contain personal information, so nothing is written to web storage.
 */
class FDCPAutocompleteCache {
    constructor({ maxEntries = 50, ttlMs = 300000, now = () => Date.now() } = {}) {
        this.maxEntries = Math.max(1, maxEntries);
        this.ttlMs = Math.max(0, ttlMs);
        this.now = now;
        this.entries = new Map();
        this.pending = new Map();
    }

    /**
     * Cache key form of a term. Ignores case, accents and spacing, and spells out letters with no decomposition,
     * so "Côté", "cote" and "COTE" share one cache entry. This assumes the data source treats them as the same
     * search; turn the cache off for a source that does not.
     */
    static normalizeTerm(term) {
        return String(term == null ? '' : term)
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .toLowerCase()
            .replace(/[œæøßł]/g, letter => ({ 'œ': 'oe', 'æ': 'ae', 'ø': 'o', 'ß': 'ss', 'ł': 'l' })[letter])
            .trim()
            .replace(/\s+/g, ' ');
    }

    static key(lang, term, skip) {
        return `${lang}|${FDCPAutocompleteCache.normalizeTerm(term)}|${skip}`;
    }

    get size() {
        return this.entries.size;
    }

    get(key) {
        const entry = this.entries.get(key);

        if (!entry) {
            return undefined;
        }

        if (this.now() - entry.storedAt >= this.ttlMs) {
            this.entries.delete(key);
            return undefined;
        }

        // Re-insert so the most recently used entry is last.
        this.entries.delete(key);
        this.entries.set(key, entry);
        return entry.value;
    }

    set(key, value) {
        this.entries.delete(key);
        this.entries.set(key, { value, storedAt: this.now() });

        while (this.entries.size > this.maxEntries) {
            this.entries.delete(this.entries.keys().next().value);
        }
    }

    /**
     * Returns the cached value, joins an identical in-flight request, or calls the loader.
     * Failures are never cached.
     */
    fetch(key, loader) {
        const cached = this.get(key);

        if (cached !== undefined) {
            return Promise.resolve(cached);
        }

        if (this.pending.has(key)) {
            return this.pending.get(key);
        }

        const request = Promise.resolve()
            .then(loader)
            .then(value => {
                if (this.pending.get(key) === request) {
                    this.pending.delete(key);
                    this.set(key, value);
                }

                return value;
            }, error => {
                if (this.pending.get(key) === request) {
                    this.pending.delete(key);
                }

                throw error;
            });

        this.pending.set(key, request);
        return request;
    }

    /** Forget in-flight requests, e.g. after they were aborted, so a retry starts a new one. */
    forgetPending() {
        this.pending.clear();
    }

    clear() {
        this.entries.clear();
        this.pending.clear();
    }
}

/**
 * Computes which rows of a virtualized list must be in the DOM.
 * `end` is exclusive.
 */
function fdcpAutocompleteWindow({ total, rowHeight, scrollTop, viewportHeight, overscan = 5 }) {
    if (!(total > 0) || !(rowHeight > 0)) {
        return { start: 0, end: 0, offsetTop: 0, totalHeight: 0 };
    }

    const first = Math.min(total - 1, Math.max(0, Math.floor(scrollTop / rowHeight)));
    const visible = Math.max(1, Math.ceil(viewportHeight / rowHeight));
    const start = Math.max(0, first - overscan);
    const end = Math.min(total, first + visible + overscan);

    return { start, end, offsetTop: start * rowHeight, totalHeight: total * rowHeight };
}

/** Returns the scroll position that brings a row fully into view with the least movement. */
function fdcpAutocompleteScrollTopForIndex({ index, rowHeight, scrollTop, viewportHeight }) {
    const top = index * rowHeight;
    const bottom = top + rowHeight;

    if (top < scrollTop) {
        return top;
    }

    if (bottom > scrollTop + viewportHeight) {
        return bottom - viewportHeight;
    }

    return scrollTop;
}

/**
 * Validates and coerces a data source response into `{ items, total }`.
 * Accepts `{ total, items }` or a bare array. Anything else throws.
 */
function fdcpAutocompleteNormalizeResult(raw, maxResults = 1000) {
    const MAX_TEXT_LENGTH = 500;
    let source;
    let total;

    if (Array.isArray(raw)) {
        source = raw;
        total = raw.length;
    } else if (raw && typeof raw === 'object' && Array.isArray(raw.items)) {
        source = raw.items;
        total = Number(raw.total);

        if (!Number.isFinite(total) || total < 0) {
            total = source.length;
        }
    } else {
        throw new TypeError('Invalid autocomplete response');
    }

    const isScalar = value => typeof value === 'string' || typeof value === 'number';
    const clean = value => String(value).slice(0, MAX_TEXT_LENGTH);
    const items = [];

    for (let index = 0; index < source.length && items.length < maxResults; index += 1) {
        const item = source[index];

        if (!item || typeof item !== 'object' || !isScalar(item.value) || !isScalar(item.text)) {
            continue;
        }

        const text = clean(item.text);

        if (text.trim() === '' || String(item.value) === '') {
            continue;
        }

        items.push({
            value: clean(item.value),
            text,
            description: isScalar(item.description) ? clean(item.description) : ''
        });
    }

    return { items, total: Math.min(Math.floor(total), maxResults) };
}

class FDCPAutocomplete {
    constructor(element) {
        this.element = element;
        this.input = element.querySelector('[data-fdcp-autocomplete-input]');
        this.clearButton = element.querySelector('[data-fdcp-autocomplete-clear]');
        this.valueInput = element.querySelector('[data-fdcp-autocomplete-value]');
        this.panel = element.querySelector('[data-fdcp-autocomplete-panel]');
        this.listbox = element.querySelector('[data-fdcp-autocomplete-listbox]');
        this.sizer = element.querySelector('[data-fdcp-autocomplete-sizer]');
        this.message = element.querySelector('[data-fdcp-autocomplete-message]');
        this.status = element.querySelector('[data-fdcp-autocomplete-status]');
        this.error = element.querySelector('[data-fdcp-autocomplete-error]');
        this.form = element.closest('form');

        if (!this.input || !this.valueInput || !this.panel || !this.listbox || !this.sizer) {
            return;
        }

        const data = element.dataset;
        const number = (value, fallback, minimum) => {
            const parsed = parseInt(value, 10);
            return Number.isFinite(parsed) ? Math.max(minimum, parsed) : fallback;
        };

        this.src = data.src || '';
        this.queryParam = data.queryParam || 'q';
        this.minLength = number(data.minLength, 2, 1);
        this.debounce = number(data.debounce, 250, 0);
        this.pageSize = number(data.pageSize, 50, 1);
        this.maxResults = number(data.maxResults, 1000, 1);
        this.cacheEnabled = data.cache !== 'false';
        this.lang = element.getAttribute('lang') || document.documentElement.lang || 'en';
        this.isRequired = data.required === 'true';
        this.overscan = 5;

        this.text = {
            loading: data.loadingText || 'Loading results...',
            noResults: data.noResultsText || 'No results found',
            oneResult: data.oneResultText || '1 result available',
            multipleResults: data.multipleResultsText || '{0} results available',
            minLength: data.minLengthText || '',
            loadError: data.loadErrorText || 'Unable to load results. Please try again.',
            required: data.requiredMessage || 'This field is required.',
            requiredSummary: data.requiredSummaryMessage || data.requiredMessage || 'This field is required.',
            invalid: data.invalidMessage || 'Select an option from the list.',
            invalidSummary: data.invalidSummaryMessage || data.invalidMessage || 'Select an option from the list.'
        };

        this.cache = new FDCPAutocompleteCache({
            maxEntries: number(data.cacheSize, 50, 1),
            ttlMs: number(data.cacheTtl, 300, 0) * 1000
        });

        /** Replaceable: ({ term, skip, take, lang, signal }) => Promise<{ items, total } | items[]> */
        this.dataSource = request => this.fetchFromSource(request);

        this.items = [];
        this.total = 0;
        this.term = '';
        this.activeIndex = -1;
        this.pendingActive = -1;
        this.rowHeight = 0;
        this.sequence = 0;
        this.controller = null;
        this.loadingPages = new Set();
        this.rendered = new Map();
        this.debounceTimer = 0;
        this.statusTimer = 0;
        this.frame = 0;
        this.hasValidationError = this.input.getAttribute('aria-invalid') === 'true';
        this.initialValue = this.valueInput.value;
        this.initialText = this.input.value;
        this.selectedText = this.valueInput.value ? this.input.value : null;

        this.bindEvents();
        this.updateClearButton();
        element.FDCPAutocompleteInstance = this;
        element.setAttribute('data-fdcp-autocomplete-initialized', 'true');

        // Lets page scripts attach a custom data source whatever order the scripts load in.
        element.dispatchEvent(new CustomEvent('fdcp-autocomplete:ready', {
            bubbles: true,
            detail: { instance: this }
        }));
    }

    bindEvents() {
        this.input.addEventListener('input', () => this.handleInput());
        this.input.addEventListener('keydown', event => this.handleKeydown(event));

        // Keep focus in the input while the pointer is used inside the panel.
        this.panel.addEventListener('mousedown', event => event.preventDefault());
        this.listbox.addEventListener('click', event => {
            const option = event.target.closest('[data-fdcp-autocomplete-option]');

            if (option) {
                this.select(parseInt(option.dataset.index, 10));
            }
        });
        this.listbox.addEventListener('scroll', () => this.scheduleRender(), { passive: true });

        if (this.clearButton) {
            this.clearButton.addEventListener('click', () => this.clear());
        }

        this.element.addEventListener('focusout', event => {
            if (event.relatedTarget && this.element.contains(event.relatedTarget)) {
                return;
            }

            this.dismiss();
            this.validate({ showError: true });
        });

        if (this.form) {
            this.form.addEventListener('submit', event => {
                if (!this.validate({ showError: true, focus: !event.defaultPrevented })) {
                    event.preventDefault();
                }
            });
            this.form.addEventListener('reset', () => this.reset());
        }

        if (typeof ResizeObserver !== 'undefined') {
            this.resizeObserver = new ResizeObserver(() => {
                if (this.isOpen()) {
                    this.rowHeight = 0;
                    this.scheduleRender();
                }
            });
            this.resizeObserver.observe(this.listbox);
        }

        // Cached results must not outlive the page view.
        window.addEventListener('pagehide', () => this.cache.clear());
    }

    /* ---------- Data ---------- */

    fetchFromSource({ term, skip, take, lang, signal }) {
        let url;

        try {
            url = new URL(this.src, document.baseURI);
        } catch (error) {
            return Promise.reject(error);
        }

        if (url.protocol !== 'http:' && url.protocol !== 'https:') {
            return Promise.reject(new Error('Unsupported autocomplete data source URL'));
        }

        url.searchParams.set(this.queryParam, term);
        url.searchParams.set('skip', String(skip));
        url.searchParams.set('take', String(take));
        url.searchParams.set('lang', lang);

        return fetch(url.toString(), {
            method: 'GET',
            credentials: 'same-origin',
            headers: {
                'Accept': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            signal
        }).then(response => {
            if (!response.ok) {
                throw new Error(`Request failed: ${response.status}`);
            }

            return response.json();
        });
    }

    handleInput() {
        const term = this.input.value.trim();

        if (this.selectedText !== null && this.input.value !== this.selectedText) {
            this.setValue(null);
        }

        this.updateClearButton();
        window.clearTimeout(this.debounceTimer);

        if (term.length < this.minLength) {
            this.abort();
            this.resetResults();

            if (term.length === 0) {
                this.close();
            } else {
                this.showMessage(this.text.minLength);
            }

            return;
        }

        this.debounceTimer = window.setTimeout(() => this.search(term), this.debounce);
    }

    search(term) {
        this.abort();
        this.resetResults();
        this.term = term;
        this.controller = typeof AbortController !== 'undefined' ? new AbortController() : null;
        this.listbox.setAttribute('aria-busy', 'true');
        this.showMessage(this.text.loading, { announce: false });
        this.loadPage(0);
    }

    loadPage(skip) {
        if (this.loadingPages.has(skip) || skip >= this.maxResults) {
            return;
        }

        this.loadingPages.add(skip);

        const sequence = this.sequence;
        const term = this.term;
        const request = {
            term,
            skip,
            take: Math.min(this.pageSize, this.maxResults - skip),
            lang: this.lang,
            signal: this.controller ? this.controller.signal : undefined
        };
        const loader = () => Promise.resolve(this.dataSource(request))
            .then(raw => fdcpAutocompleteNormalizeResult(raw, this.maxResults));
        const result = this.cacheEnabled
            ? this.cache.fetch(FDCPAutocompleteCache.key(this.lang, term, skip), loader)
            : Promise.resolve().then(loader);

        result.then(page => {
            if (sequence !== this.sequence) {
                return;
            }

            this.loadingPages.delete(skip);
            this.applyPage(skip, page);
        }, error => {
            if (sequence !== this.sequence) {
                return;
            }

            this.loadingPages.delete(skip);
            this.listbox.removeAttribute('aria-busy');

            if (error && error.name === 'AbortError') {
                return;
            }

            if (skip === 0) {
                this.resetResults();
                this.showMessage(this.text.loadError);
            }

            this.element.dispatchEvent(new CustomEvent('fdcp-autocomplete:error', {
                bubbles: true,
                detail: { term, skip, error }
            }));
        });
    }

    applyPage(skip, page) {
        let total = Math.min(page.total, this.maxResults);

        page.items.forEach((item, offset) => {
            if (skip + offset < this.maxResults) {
                this.items[skip + offset] = item;
            }
        });

        // A short page means the source has no more rows, whatever total it reported.
        if (page.items.length < this.pageSize && skip + page.items.length < total) {
            total = skip + page.items.length;
        }

        this.total = total;
        this.listbox.removeAttribute('aria-busy');

        if (total === 0) {
            this.showMessage(this.text.noResults);
        } else {
            this.showResults();

            // An arrow key pressed while the first page was loading asked for an active option.
            if (skip === 0 && this.pendingActive >= 0) {
                this.setActive(this.pendingActive);
                this.pendingActive = -1;
            }

            if (skip === 0) {
                this.announce(total === 1
                    ? this.text.oneResult
                    : this.text.multipleResults.replace('{0}', String(total)));
            }
        }

        this.element.dispatchEvent(new CustomEvent('fdcp-autocomplete:loaded', {
            bubbles: true,
            detail: { term: this.term, skip, total, count: page.items.length }
        }));
    }

    abort() {
        this.sequence += 1;
        this.loadingPages.clear();
        this.cache.forgetPending();

        if (this.controller) {
            this.controller.abort();
            this.controller = null;
        }
    }

    resetResults() {
        this.items = [];
        this.total = 0;
        this.term = '';
        this.activeIndex = -1;
        this.pendingActive = -1;
        this.input.removeAttribute('aria-activedescendant');
        this.listbox.removeAttribute('aria-busy');
        this.clearRendered();
        this.listbox.scrollTop = 0;
    }

    /* ---------- Panel ---------- */

    isOpen() {
        return !this.panel.hidden;
    }

    open() {
        if (this.isOpen()) {
            return;
        }

        this.panel.hidden = false;
        this.input.setAttribute('aria-expanded', 'true');
        this.updatePlacement();
    }

    close() {
        window.clearTimeout(this.debounceTimer);

        if (!this.isOpen()) {
            return;
        }

        this.panel.hidden = true;
        this.activeIndex = -1;
        this.input.setAttribute('aria-expanded', 'false');
        this.input.removeAttribute('aria-activedescendant');
        this.element.classList.remove('fdcp-autocomplete--open-above');
    }

    /** Closes the list and cancels lookups in flight, so late results cannot reopen it. */
    dismiss() {
        window.clearTimeout(this.debounceTimer);
        this.abort();
        this.listbox.removeAttribute('aria-busy');
        this.pendingActive = -1;
        this.close();
    }

    showMessage(text, { announce = true } = {}) {
        this.listbox.hidden = true;

        if (this.message) {
            this.message.textContent = text;
            this.message.hidden = !text;
        }

        if (!text) {
            this.close();
            return;
        }

        this.open();
        this.updatePlacement();

        if (announce) {
            this.announce(text);
        }
    }

    showResults() {
        if (this.message) {
            this.message.hidden = true;
            this.message.textContent = '';
        }

        this.listbox.hidden = false;
        this.open();
        this.render();
        this.updatePlacement();
    }

    updatePlacement() {
        if (!this.isOpen()) {
            return;
        }

        this.element.classList.remove('fdcp-autocomplete--open-above');

        const inputRect = this.input.getBoundingClientRect();
        const panelHeight = this.panel.getBoundingClientRect().height;
        const viewportHeight = window.innerHeight || document.documentElement.clientHeight;
        const spaceBelow = viewportHeight - inputRect.bottom;
        const spaceAbove = inputRect.top;

        this.element.classList.toggle(
            'fdcp-autocomplete--open-above',
            panelHeight > spaceBelow && spaceAbove > spaceBelow
        );
    }

    announce(text) {
        if (!this.status) {
            return;
        }

        // Debounced so fast typing does not queue a message per keystroke.
        window.clearTimeout(this.statusTimer);
        this.status.textContent = '';
        this.statusTimer = window.setTimeout(() => {
            this.status.textContent = text;
        }, 150);
    }

    /* ---------- Virtualized list ---------- */

    scheduleRender() {
        if (this.frame) {
            return;
        }

        this.frame = window.requestAnimationFrame(() => {
            this.frame = 0;
            this.render();
        });
    }

    measureRowHeight() {
        if (this.rowHeight > 0) {
            return;
        }

        const probe = this.createOption(0, { text: ' ', description: ' ', value: '' });
        probe.removeAttribute('id');
        probe.setAttribute('aria-hidden', 'true');
        this.sizer.appendChild(probe);
        this.rowHeight = probe.getBoundingClientRect().height;
        this.sizer.removeChild(probe);
        this.clearRendered();
    }

    render() {
        if (!this.isOpen() || this.listbox.hidden || this.total === 0) {
            return;
        }

        this.measureRowHeight();

        if (!(this.rowHeight > 0)) {
            return;
        }

        // Size the list before measuring it: until the sizer has its height the viewport reads as empty.
        this.sizer.style.height = `${this.total * this.rowHeight}px`;

        const view = fdcpAutocompleteWindow({
            total: this.total,
            rowHeight: this.rowHeight,
            scrollTop: this.listbox.scrollTop,
            viewportHeight: this.listbox.clientHeight,
            overscan: this.overscan
        });
        const wanted = new Set();

        for (let index = view.start; index < view.end; index += 1) {
            wanted.add(index);
        }

        // The active option must always exist so aria-activedescendant never dangles.
        if (this.activeIndex >= 0 && this.activeIndex < this.total) {
            wanted.add(this.activeIndex);
        }

        this.rendered.forEach((option, index) => {
            const isLoaded = option.dataset.loaded === 'true';

            if (!wanted.has(index) || (!isLoaded && this.items[index])) {
                this.sizer.removeChild(option);
                this.rendered.delete(index);
            }
        });

        const fragment = document.createDocumentFragment();

        wanted.forEach(index => {
            let option = this.rendered.get(index);

            if (!option) {
                option = this.createOption(index, this.items[index]);
                this.rendered.set(index, option);
                fragment.appendChild(option);

                if (!this.items[index]) {
                    this.loadPage(Math.floor(index / this.pageSize) * this.pageSize);
                }
            }

            option.setAttribute('aria-selected', index === this.activeIndex ? 'true' : 'false');
            option.setAttribute('aria-setsize', String(this.total));
            option.classList.toggle('is-active', index === this.activeIndex);
        });

        this.sizer.appendChild(fragment);
    }

    createOption(index, item) {
        const option = document.createElement('div');

        option.className = 'fdcp-autocomplete__option';
        option.id = `${this.listbox.id}_opt_${index}`;
        option.setAttribute('role', 'option');
        option.setAttribute('aria-posinset', String(index + 1));
        option.setAttribute('data-fdcp-autocomplete-option', '');
        option.dataset.index = String(index);
        // CSSOM property writes are permitted by the nonce-based CSP; style attributes are not.
        option.style.transform = `translateY(${index * this.rowHeight}px)`;

        // Data source text is only ever inserted as text, never as HTML.
        const text = document.createElement('span');
        text.className = 'fdcp-autocomplete__option-text';

        if (item) {
            option.dataset.loaded = 'true';
            text.textContent = item.text;
            option.appendChild(text);

            if (item.description) {
                const description = document.createElement('span');
                description.className = 'fdcp-autocomplete__option-description';
                description.textContent = item.description;
                option.appendChild(description);
            }
        } else {
            option.dataset.loaded = 'false';
            option.classList.add('fdcp-autocomplete__option--loading');
            option.setAttribute('aria-disabled', 'true');
            text.textContent = this.text.loading;
            option.appendChild(text);
        }

        return option;
    }

    clearRendered() {
        this.rendered.clear();

        while (this.sizer.firstChild) {
            this.sizer.removeChild(this.sizer.firstChild);
        }
    }

    setActive(index) {
        if (this.total === 0) {
            return;
        }

        this.activeIndex = Math.min(this.total - 1, Math.max(0, index));
        this.measureRowHeight();
        this.listbox.scrollTop = fdcpAutocompleteScrollTopForIndex({
            index: this.activeIndex,
            rowHeight: this.rowHeight,
            scrollTop: this.listbox.scrollTop,
            viewportHeight: this.listbox.clientHeight
        });
        this.render();

        const option = this.rendered.get(this.activeIndex);

        if (option) {
            this.input.setAttribute('aria-activedescendant', option.id);
        }
    }

    rowsPerPage() {
        return this.rowHeight > 0 ? Math.max(1, Math.floor(this.listbox.clientHeight / this.rowHeight)) : 10;
    }

    /* ---------- Keyboard ---------- */

    handleKeydown(event) {
        // Ignore keys used by an IME while composing.
        if (event.isComposing || event.keyCode === 229) {
            return;
        }

        const listOpen = this.isOpen() && !this.listbox.hidden && this.total > 0;

        switch (event.key) {
            case 'ArrowDown':
                event.preventDefault();

                if (listOpen) {
                    this.setActive(this.activeIndex + 1);
                } else {
                    this.reopen(event.altKey ? -1 : 0);
                }
                break;
            case 'ArrowUp':
                if (listOpen) {
                    event.preventDefault();
                    this.setActive(this.activeIndex < 0 ? this.total - 1 : this.activeIndex - 1);
                }
                break;
            case 'PageDown':
                if (listOpen) {
                    event.preventDefault();
                    this.setActive(Math.max(0, this.activeIndex) + this.rowsPerPage());
                }
                break;
            case 'PageUp':
                if (listOpen) {
                    event.preventDefault();
                    this.setActive(Math.max(0, this.activeIndex) - this.rowsPerPage());
                }
                break;
            case 'Home':
            case 'End':
                // Unmodified Home/End stay with the text caret.
                if (listOpen && event.ctrlKey) {
                    event.preventDefault();
                    this.setActive(event.key === 'Home' ? 0 : this.total - 1);
                }
                break;
            case 'Enter':
                if (listOpen && this.activeIndex >= 0) {
                    event.preventDefault();
                    this.select(this.activeIndex);
                }
                break;
            case 'Escape':
                if (this.isOpen() || this.loadingPages.size > 0) {
                    event.preventDefault();
                    this.dismiss();
                } else if (this.input.value !== '') {
                    event.preventDefault();
                    this.clear();
                }
                break;
            case 'Tab':
                this.dismiss();
                break;
            default:
                break;
        }
    }

    /** Opens the list for the current text; `activeIndex` of -1 leaves no option active. */
    reopen(activeIndex) {
        const term = this.input.value.trim();

        if (term.length < this.minLength) {
            if (term.length > 0) {
                this.showMessage(this.text.minLength);
            }

            return;
        }

        if (this.total > 0 && term === this.term) {
            this.showResults();

            if (activeIndex >= 0) {
                this.setActive(activeIndex);
            }

            return;
        }

        // Already loading this term: keep the request and remember which option to activate.
        if (term !== this.term || !this.loadingPages.has(0)) {
            window.clearTimeout(this.debounceTimer);
            this.search(term);
        }

        this.pendingActive = activeIndex;
    }

    /* ---------- Selection ---------- */

    select(index) {
        const item = this.items[index];

        if (!item) {
            return;
        }

        this.input.value = item.text;
        this.setValue(item);
        this.close();
        this.updateClearButton();
        this.validate();
    }

    setValue(item) {
        const previous = this.valueInput.value;

        this.valueInput.value = item ? item.value : '';
        this.selectedText = item ? item.text : null;

        if (previous !== this.valueInput.value) {
            this.element.dispatchEvent(new CustomEvent('fdcp-autocomplete:change', {
                bubbles: true,
                detail: {
                    value: this.valueInput.value,
                    text: item ? item.text : '',
                    item: item || null
                }
            }));
        }
    }

    clear() {
        // Move focus before the clear button is hidden, so focus never falls out of the component.
        this.input.focus();
        window.clearTimeout(this.debounceTimer);
        this.abort();
        this.resetResults();
        this.close();
        this.input.value = '';
        this.setValue(null);
        this.updateClearButton();
        this.validate();
    }

    reset() {
        window.clearTimeout(this.debounceTimer);
        this.abort();
        this.resetResults();
        this.close();
        this.input.value = this.initialText;
        this.valueInput.value = this.initialValue;
        this.selectedText = this.initialValue ? this.initialText : null;
        this.updateClearButton();
        this.setValidationState(true, false, '', '');
    }

    updateClearButton() {
        if (this.clearButton) {
            this.clearButton.hidden = this.input.value === '' || this.input.disabled;
        }
    }

    /* ---------- Validation ---------- */

    validate({ showError = false, focus = false } = {}) {
        if (this.input.disabled) {
            return true;
        }

        const hasText = this.input.value.trim() !== '';
        const hasValue = this.valueInput.value !== '';
        let message = '';
        let summary = '';

        if (hasText && !hasValue) {
            message = this.text.invalid;
            summary = this.text.invalidSummary;
        } else if (!hasValue && this.isRequired) {
            message = this.text.required;
            summary = this.text.requiredSummary;
        }

        const isValid = message === '';

        this.setValidationState(isValid, !isValid && (showError || this.hasValidationError), message, summary);

        if (!isValid && focus) {
            this.input.focus();
        }

        return isValid;
    }

    setValidationState(isValid, shouldShowError, message, summary) {
        this.hasValidationError = shouldShowError;

        if (shouldShowError) {
            this.input.setAttribute('aria-invalid', 'true');

            if (this.error) {
                this.error.textContent = message;
                this.error.hidden = false;
            }

            this.input.dispatchEvent(new CustomEvent('gcdsError', {
                bubbles: true,
                composed: true,
                detail: { message: summary }
            }));
        } else if (isValid) {
            this.input.removeAttribute('aria-invalid');

            if (this.error) {
                this.error.hidden = true;
                this.error.textContent = '';
            }

            this.input.dispatchEvent(new CustomEvent('gcdsValid', {
                bubbles: true,
                composed: true
            }));
        }
    }
}

if (typeof document !== 'undefined') {
    const initFDCPAutocomplete = () => {
        document.querySelectorAll('[data-fdcp-autocomplete]').forEach(element => {
            if (!element.FDCPAutocompleteInstance) {
                element.FDCPAutocompleteInstance = new FDCPAutocomplete(element);
            }
        });
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initFDCPAutocomplete);
    } else {
        initFDCPAutocomplete();
    }
}

if (typeof window !== 'undefined') {
    window.FDCPAutocomplete = FDCPAutocomplete;
}

// Unit-test hook; `module` does not exist in the browser bundle.
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        FDCPAutocomplete,
        FDCPAutocompleteCache,
        fdcpAutocompleteWindow,
        fdcpAutocompleteScrollTopForIndex,
        fdcpAutocompleteNormalizeResult
    };
}
