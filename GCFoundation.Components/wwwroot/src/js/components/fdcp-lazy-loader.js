/**
 * Loads the scripts of large components only on pages that use them.
 *
 * Each entry is built to its own file next to foundation.min.js (see paths.js.lazy in gulpfile.js) instead of
 * being bundled. When the page has an element that matches the selector, the file is added as a normal
 * same-origin script, so the default "script-src 'self'" policy covers it and no nonce is needed.
 * The build replaces each version placeholder with a hash of the file, so browsers fetch it again only when it changes.
 *
 * Scripts that need a component should listen for its ready event (for example "fdcp-autocomplete:ready"),
 * or call window.FDCPLazy.load(name) and wait for the promise.
 */
(function () {
    if (typeof document === 'undefined' || typeof window === 'undefined') {
        return;
    }

    const components = {
        autocomplete: {
            selector: '[data-fdcp-autocomplete]',
            file: 'fdcp-autocomplete.min.js',
            version: '__FDCP_LAZY_VERSION_fdcp-autocomplete__'
        }
    };

    // Resolve file names against the folder foundation.min.js was served from, so virtual directories and
    // custom static file paths work. document.currentScript is only set while this file first runs.
    const bundle = document.currentScript || document.querySelector('script[src*="foundation.min.js"]');
    const baseUrl = bundle && bundle.src ? bundle.src : document.baseURI;
    const loading = {};

    function scriptUrl(component) {
        const url = new URL(component.file, baseUrl);
        url.search = '';
        url.hash = '';

        if (component.version.indexOf('__') !== 0) {
            url.searchParams.set('v', component.version);
        }

        return url.href;
    }

    function load(name) {
        const component = components[name];

        if (!component) {
            return Promise.reject(new Error(`Unknown lazy component "${name}".`));
        }

        if (!loading[name]) {
            loading[name] = new Promise((resolve, reject) => {
                const script = document.createElement('script');
                script.src = scriptUrl(component);
                script.async = false;
                script.setAttribute('data-fdcp-lazy', name);
                script.addEventListener('load', () => resolve());
                script.addEventListener('error', () => {
                    // Allow a later call to try again, for example after a network blip.
                    delete loading[name];
                    script.remove();
                    reject(new Error(`Could not load ${component.file}.`));
                });
                document.head.appendChild(script);
            });
        }

        return loading[name];
    }

    function loadUsedComponents() {
        Object.keys(components).forEach(name => {
            if (document.querySelector(components[name].selector)) {
                load(name).catch(error => console.error(error));
            }
        });
    }

    window.FDCPLazy = { load };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', loadUsedComponents);
    } else {
        loadUsedComponents();
    }
})();
