import { expect, test } from '@playwright/test';

const defaultBaseUrl = process.env.CODELENS_BASE_URL ?? 'https://codelens.lab.rvmtech.com.br';

test.describe('CodeLens UI e API', () => {
  test.skip(
    process.env.CODELENS_RUN_SMOKE !== '1',
    'Defina CODELENS_RUN_SMOKE=1 para rodar o smoke contra um ambiente real.',
  );

  test('home page carrega com status 200', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/`);
    expect(response.status()).toBe(200);
  });

  test('home page contem elemento de navegacao', async ({ page, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    await page.goto(currentBaseUrl);
    const nav = page.locator('nav, header, [role="navigation"]');
    await expect(nav.first()).toBeVisible();
  });

  test('GET /api/analysis/current — retorna 200 ou 404 (sem analise ativa)', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/analysis/current`);
    expect([200, 404]).toContain(response.status());
  });

  test('GET /api/metrics — retorna 200 ou 404', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/metrics`);
    expect([200, 404]).toContain(response.status());
  });

  test('POST /api/analyze — sem body retorna 400', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.post(`${currentBaseUrl}/api/analyze`, { data: {} });
    expect([400, 422]).toContain(response.status());
  });

  test('GET /api/deps — retorna 200 ou 404', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/deps`);
    expect([200, 404]).toContain(response.status());
  });
});
