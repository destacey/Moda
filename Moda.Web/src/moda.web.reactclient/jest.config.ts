import type { Config } from 'jest'
import nextJest from 'next/jest.js'

const createJestConfig = nextJest({
  // Provide the path to your Next.js app to load next.config.js and .env files in your test environment
  dir: './',
})

// Add any custom config to be passed to Jest
const config: Config = {
  coverageProvider: 'v8',
  testEnvironment: 'jsdom',
  collectCoverage: true,
  // Add more setup options before each test is run
  setupFilesAfterEnv: ['./src/jest.setup.ts'],
  transform: {
    '^.+\\.(ts|tsx|js|jsx)$': [
      'ts-jest',
      {
        useESM: true,
        jsx: 'react-jsx', // nextjs needs the main setting to be "preserve". This is a workaround.
      } as Record<string, unknown>,
    ],
  },
  // Add other ESM packages here if needed
  transformIgnorePatterns: [
    'node_modules/(?!(rehype-raw|react-markdown|remark-gfm|remark-parse|unified|mdast-util-to-string)/)',
  ],
  testPathIgnorePatterns: ['./.next/', './node_modules/'],
}

// createJestConfig is exported this way to ensure that next/jest can load the Next.js config which is async
export default createJestConfig(config)
