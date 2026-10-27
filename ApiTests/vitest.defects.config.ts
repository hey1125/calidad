import { defineConfig } from 'vitest/config';

// Separate config: these tests assert how the API SHOULD behave and are expected to fail today
export default defineConfig({
  test: {
    environment: 'node',
    include: ['tests/defects/**/*.defect.ts'],
    testTimeout: 15000,
  },
});