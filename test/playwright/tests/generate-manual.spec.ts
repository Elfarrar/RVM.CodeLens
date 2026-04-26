/**
 * RVM.CodeLens — Gerador de Manual Visual
 *
 * Playwright script que navega por todas as telas do sistema de analise estatica,
 * captura screenshots em desktop e mobile, e gera as imagens para o manual.
 *
 * Uso:
 *   cd test/playwright
 *   npx playwright test tests/generate-manual.spec.ts --reporter=list
 */
import { test, type Page } from '@playwright/test';
import path from 'path';

const BASE_URL = process.env.CODELENS_BASE_URL ?? 'https://codelens.lab.rvmtech.com.br';
const SCREENSHOTS_DIR = path.resolve(__dirname, '../../../docs/screenshots');

/** Captura desktop (1280x800) + mobile (390x844) */
async function capture(page: Page, name: string, opts?: { fullPage?: boolean }) {
  const fullPage = opts?.fullPage ?? true;
  await page.screenshot({ path: path.join(SCREENSHOTS_DIR, `${name}--desktop.png`), fullPage });
  await page.setViewportSize({ width: 390, height: 844 });
  await page.screenshot({ path: path.join(SCREENSHOTS_DIR, `${name}--mobile.png`), fullPage });
  await page.setViewportSize({ width: 1280, height: 800 });
}

test.describe('RVM.CodeLens — Manual Visual', () => {
  test('01 Home', async ({ page }) => {
    await page.goto(`${BASE_URL}/`);
    await page.waitForLoadState('networkidle');
    await capture(page, '01-home');
  });

  test('02 Dashboard', async ({ page }) => {
    await page.goto(`${BASE_URL}/dashboard`);
    await page.waitForLoadState('networkidle');
    await capture(page, '02-dashboard');
  });

  test('03 Metricas', async ({ page }) => {
    await page.goto(`${BASE_URL}/metrics`);
    await page.waitForLoadState('networkidle');
    await capture(page, '03-metrics');
  });

  test('04 Dependencias', async ({ page }) => {
    await page.goto(`${BASE_URL}/deps`);
    await page.waitForLoadState('networkidle');
    await capture(page, '04-dependencies');
  });

  test('05 Hotspots', async ({ page }) => {
    await page.goto(`${BASE_URL}/hotspots`);
    await page.waitForLoadState('networkidle');
    await capture(page, '05-hotspots');
  });

  test('06 Arquitetura', async ({ page }) => {
    await page.goto(`${BASE_URL}/architecture`);
    await page.waitForLoadState('networkidle');
    await capture(page, '06-architecture');
  });
});
