function initializeAccountMenuIcon() {
    const accountMenus = document.querySelectorAll(
        'gcds-nav-group.fdcp-user-login__account-menu'
    );

    accountMenus.forEach(accountMenu => {
        customElements.whenDefined('gcds-nav-group').then(async () => {
            await accountMenu.componentOnReady?.();
            const root = accountMenu.shadowRoot;
            if (!root || accountMenu.dataset.accountMenuIconStyled === 'true') {
                return;
            }

            const styles = `
                .gcds-trigger--dropdown {
                    padding-inline-start: calc(
                        var(--gcds-spacing-200) + 1.75rem
                    );
                    position: relative;
                }

                .gcds-trigger--dropdown::before {
                    content: '\\f007';
                    font-family: "Font Awesome 6 Free";
                    font-size: 1.25rem;
                    font-style: normal;
                    font-weight: 900;
                    inset-inline-start: var(--gcds-spacing-200);
                    line-height: 1;
                    position: absolute;
                    top: 50%;
                    transform: translateY(-50%);
                }
            `;

            if (
                'adoptedStyleSheets' in root &&
                typeof CSSStyleSheet.prototype.replaceSync === 'function'
            ) {
                const styleSheet = new CSSStyleSheet();
                styleSheet.replaceSync(styles);
                root.adoptedStyleSheets = [
                    ...root.adoptedStyleSheets,
                    styleSheet
                ];
            } else {
                const style = document.createElement('style');
                style.textContent = styles;
                root.append(style);
            }

            accountMenu.dataset.accountMenuIconStyled = 'true';
        });
    });
}

document.addEventListener('DOMContentLoaded', initializeAccountMenuIcon);
