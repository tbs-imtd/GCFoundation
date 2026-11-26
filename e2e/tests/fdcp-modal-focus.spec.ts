import { test, expect, type Page } from '@playwright/test';

/**
 * Keyboard focus containment for <fdcp-modal> (Tab / Shift+Tab wrap handler).
 *
 * The demo page only ships simple modals, so most tests inject a <dialog> fixture
 * with the content under test and wire it up with the page's own FDCPModal class.
 * The last test covers the real CRUD demo delete modal.
 */

const CLOSE = '<button type="button" id="m-close" class="fdcp-modal-close">Close</button>';

interface FixtureOptions {
  closeHtml?: string;
  /** Renders the body the way the tag helper does for `scrollable`. */
  scrollable?: boolean;
}

async function openFixtureModal(
  page: Page,
  bodyHtml: string,
  footerHtml = '',
  { closeHtml = CLOSE, scrollable = false }: FixtureOptions = {}
) {
  await page.goto('/components/modal');
  await page.waitForFunction(() => typeof (window as any).FDCPModal === 'function');

  await page.evaluate(
    ({ close, body, footer, scrollable }) => {
      const dialogClass = scrollable ? 'modal modal--scrollable' : 'modal';
      const bodyAttrs = scrollable
        ? 'class="modal__body modal__body--scrollable" tabindex="0" role="region" aria-label="Body"'
        : 'class="modal__body"';
      document.body.insertAdjacentHTML(
        'beforeend',
        `<button type="button" id="fixture-trigger">Open fixture</button>
         <dialog class="${dialogClass}" id="fixture-modal" modal-id="fixture-modal"
                 aria-modal="true" aria-label="Fixture" tabindex="-1">
           <div class="modal__header">${close}</div>
           <div id="fixture-body" ${bodyAttrs}>${body}</div>
           <div class="modal__footer">${footer}</div>
         </dialog>`
      );
      const FDCPModal = (window as any).FDCPModal;
      (window as any).__fixture = new FDCPModal(document.getElementById('fixture-modal'));
    },
    { close: closeHtml, body: bodyHtml, footer: footerHtml, scrollable }
  );

  await page.locator('#fixture-trigger').focus();
  await page.evaluate(() => {
    (window as any).__fixture.show(document.getElementById('fixture-trigger'));
  });
  await expect(page.locator('#fixture-modal')).toBeVisible();
}

test.describe('FDCP modal focus containment', () => {
  test('moves focus to Close on open and returns it to the trigger on close @core', async ({ page }) => {
    await openFixtureModal(page, '<input id="a" aria-label="A" />');

    await expect(page.locator('#m-close')).toBeFocused();
    await expect(page.locator('#fixture-modal')).toHaveAttribute('aria-modal', 'true');

    await page.keyboard.press('Escape');

    await expect(page.locator('#fixture-modal')).toBeHidden();
    await expect(page.locator('#fixture-trigger')).toBeFocused();
  });

  test('wraps Tab from last to first and Shift+Tab from first to last @core', async ({ page }) => {
    await openFixtureModal(
      page,
      '<input id="a" aria-label="A" /><input id="b" aria-label="B" />',
      '<button type="button" id="ok">OK</button>'
    );

    await expect(page.locator('#m-close')).toBeFocused();

    await page.keyboard.press('Shift+Tab');
    await expect(page.locator('#ok')).toBeFocused();

    await page.keyboard.press('Tab');
    await expect(page.locator('#m-close')).toBeFocused();

    await page.keyboard.press('Tab');
    await expect(page.locator('#a')).toBeFocused();
    await page.keyboard.press('Tab');
    await expect(page.locator('#b')).toBeFocused();
    await page.keyboard.press('Tab');
    await expect(page.locator('#ok')).toBeFocused();
  });

  test('skips disabled, hidden and tabindex=-1 elements when wrapping', async ({ page }) => {
    await openFixtureModal(
      page,
      `<input id="a" aria-label="A" />
       <button type="button" id="skip-neg" tabindex="-1">Skip</button>
       <button type="button" id="skip-dis" disabled>Disabled</button>
       <div hidden><button type="button" id="skip-hid">Hidden</button></div>`
    );

    await page.locator('#a').focus();
    await page.keyboard.press('Tab');
    await expect(page.locator('#m-close')).toBeFocused();
  });

  test('keeps focus on the dialog when nothing focusable is inside', async ({ page }) => {
    await openFixtureModal(page, '<p>Only text</p>');

    // Remove the only focusable element, then press Tab.
    await page.evaluate(() => document.getElementById('m-close')!.remove());
    await page.evaluate(() => document.getElementById('fixture-modal')!.focus());
    await expect(page.locator('#fixture-modal')).toBeFocused();

    await page.keyboard.press('Tab');
    await expect(page.locator('#fixture-modal')).toBeFocused();
    await page.keyboard.press('Shift+Tab');
    await expect(page.locator('#fixture-modal')).toBeFocused();
  });

  test.describe('radio groups', () => {
    const radios = (checked?: string) => `
      <fieldset>
        <legend>Choice</legend>
        ${['r1', 'r2', 'r3']
          .map(
            (id) =>
              `<label><input type="radio" name="choice" id="${id}" ${id === checked ? 'checked' : ''} /> ${id}</label>`
          )
          .join('')}
      </fieldset>`;

    test('wraps from the checked radio when the group is last', async ({ page }) => {
      await openFixtureModal(page, radios('r2'));

      await page.locator('#r2').focus();
      await page.keyboard.press('Tab');
      await expect(page.locator('#m-close')).toBeFocused();

      await page.keyboard.press('Shift+Tab');
      await expect(page.locator('#r2')).toBeFocused();
    });

    test('uses the first radio as the tab stop when none is checked', async ({ page }) => {
      await openFixtureModal(page, radios());

      await page.keyboard.press('Tab');
      await expect(page.locator('#r1')).toBeFocused();

      await page.keyboard.press('Tab');
      await expect(page.locator('#m-close')).toBeFocused();
    });

    test('arrow keys move within the group without escaping the modal', async ({ page }) => {
      await openFixtureModal(page, radios('r1'));

      await page.locator('#r1').focus();
      await page.keyboard.press('ArrowDown');
      await expect(page.locator('#r2')).toBeFocused();
      await expect(page.locator('#r2')).toBeChecked();

      await page.keyboard.press('Tab');
      await expect(page.locator('#m-close')).toBeFocused();
    });
  });

  test.describe('rich text editors', () => {
    const editor = `
      <div role="toolbar" aria-label="Formatting">
        <button type="button" id="bold">Bold</button>
      </div>
      <div id="editor" contenteditable="true" role="textbox" aria-multiline="true" aria-label="Body"></div>`;

    test('reaches toolbar and editable area, then wraps', async ({ page }) => {
      await openFixtureModal(page, editor);

      await page.keyboard.press('Tab');
      await expect(page.locator('#bold')).toBeFocused();
      await page.keyboard.press('Tab');
      await expect(page.locator('#editor')).toBeFocused();

      // Editor is the last tab stop, so Tab wraps to Close.
      await page.keyboard.press('Tab');
      await expect(page.locator('#m-close')).toBeFocused();

      await page.keyboard.press('Shift+Tab');
      await expect(page.locator('#editor')).toBeFocused();
    });

    test('does not trap typing in the editable area', async ({ page }) => {
      await openFixtureModal(page, editor);

      await page.locator('#editor').click();
      await page.keyboard.type('hello');
      await expect(page.locator('#editor')).toHaveText('hello');
    });
  });

  test.describe('iframes', () => {
    const iframe = `
      <input id="before" aria-label="Before" />
      <iframe id="frame" title="Embedded form"
        srcdoc="<input id='f1' aria-label='F1'><input id='f2' aria-label='F2'>"></iframe>`;

    test('lets Tab pass through iframe content before wrapping', async ({ page }) => {
      await openFixtureModal(page, iframe, '<button type="button" id="ok">OK</button>');
      const frame = page.frameLocator('#frame');

      await page.locator('#before').focus();
      await page.keyboard.press('Tab');
      await expect(frame.locator('#f1')).toBeFocused();
      await page.keyboard.press('Tab');
      await expect(frame.locator('#f2')).toBeFocused();
      await page.keyboard.press('Tab');
      await expect(page.locator('#ok')).toBeFocused();

      // Last element wraps to the first.
      await page.keyboard.press('Tab');
      await expect(page.locator('#m-close')).toBeFocused();
    });

    test('wraps Shift+Tab from first element to the last', async ({ page }) => {
      await openFixtureModal(page, iframe, '<button type="button" id="ok">OK</button>');

      await expect(page.locator('#m-close')).toBeFocused();
      await page.keyboard.press('Shift+Tab');
      await expect(page.locator('#ok')).toBeFocused();
    });

    test('wraps Tab out of an iframe that is the last focusable element', async ({ page }) => {
      await openFixtureModal(page, iframe);
      const frame = page.frameLocator('#frame');

      await frame.locator('#f2').focus();
      await page.keyboard.press('Tab');
      await expect(page.locator('#m-close')).toBeFocused();
    });

    test('wraps Shift+Tab out of an iframe that is the first focusable element', async ({ page }) => {
      await openFixtureModal(
        page,
        `<iframe id="frame" title="Embedded form"
          srcdoc="<input id='f1' aria-label='F1'><input id='f2' aria-label='F2'>"></iframe>`,
        '<button type="button" id="ok">OK</button>',
        { closeHtml: '' }
      );
      const frame = page.frameLocator('#frame');

      await frame.locator('#f1').focus();
      await page.keyboard.press('Shift+Tab');
      await expect(page.locator('#ok')).toBeFocused();
    });
  });

  test.describe('scrollable body', () => {
    const footer = '<button type="button" id="ok">OK</button>';

    test('is not a tab stop when its content does not overflow', async ({ page }) => {
      await openFixtureModal(page, '<p>Short text</p>', footer, { scrollable: true });

      await expect(page.locator('#fixture-body')).not.toHaveAttribute('tabindex');
      await expect(page.locator('#m-close')).toBeFocused();
      await page.keyboard.press('Tab');
      await expect(page.locator('#ok')).toBeFocused();
    });

    test('is a tab stop when its content overflows', async ({ page }) => {
      // The site's CSP blocks inline styles, so make the content tall with real text.
      const tallContent = Array.from({ length: 60 }, (_, i) => `<p>Paragraph ${i + 1}</p>`).join('');
      await openFixtureModal(page, tallContent, footer, { scrollable: true });

      await expect(page.locator('#fixture-body')).toHaveAttribute('tabindex', '0');
      await page.keyboard.press('Tab');
      await expect(page.locator('#fixture-body')).toBeFocused();
      await page.keyboard.press('Tab');
      await expect(page.locator('#ok')).toBeFocused();
    });
  });

  test('finds buttons inside GCDS shadow roots @core', async ({ page }) => {
    await page.goto('/components/modal');
    await page.getByRole('button', { name: /open/i }).first().click();

    const dialog = page.locator('dialog[open]');
    await expect(dialog).toBeVisible();

    const innerButtonsReceiveFocus = await dialog.evaluate((d) =>
      Array.from(d.querySelectorAll('gcds-button')).every((host) => {
        const inner = host.shadowRoot?.querySelector('button, a');
        if (!(inner instanceof HTMLElement)) return false;
        inner.focus();
        return host.shadowRoot!.activeElement === inner;
      })
    );
    expect(innerButtonsReceiveFocus).toBe(true);

    const focusIsInsideDialog = () =>
      page.evaluate(() => {
        let a: Node | null = document.activeElement;
        const dlg = document.querySelector('dialog[open]');
        while (a) {
          if (a === dlg) return true;
          a = (a as Element).parentElement ?? ((a.getRootNode() as ShadowRoot).host ?? null);
        }
        return false;
      });

    // Tab and Shift+Tab many times: focus must never leave the dialog.
    for (const key of ['Tab', 'Shift+Tab']) {
      for (let i = 0; i < 8; i++) {
        await page.keyboard.press(key);
        expect(await focusIsInsideDialog()).toBe(true);
      }
    }
  });

  test('CRUD demo delete modal traps focus and names the Delete button @core', async ({ page }) => {
    await page.goto('/template/crud/demo');

    const trigger = page.locator('gcds-button.fdcp-modal-open[modal-id="delete-employee-modal"]').first();
    await expect(trigger).toBeVisible();
    const employeeName = await trigger.getAttribute('data-employee-name');
    expect(employeeName).toBeTruthy();

    await trigger.click();
    const dialog = page.locator('dialog[modal-id="delete-employee-modal"]');
    await expect(dialog).toBeVisible();

    const close = dialog.locator('gcds-button.fdcp-modal-close').first();
    const cancel = dialog.locator('gcds-button[button-id="delete-employee-cancel"]');
    await expect(close).toBeFocused();

    await expect(dialog.getByRole('button', { name: `Delete ${employeeName}`, exact: true })).toBeVisible();

    await page.keyboard.press('Shift+Tab');
    await expect(cancel).toBeFocused();
    await page.keyboard.press('Tab');
    await expect(close).toBeFocused();

    await page.keyboard.press('Escape');
    await expect(dialog).toBeHidden();
    await expect(trigger).toBeFocused();
  });
});
