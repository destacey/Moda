const nextJest = require('next/jest')

const createJestConfig = nextJest({
  dir: './'
})

const customJestConfig = {
  testEnvironment: 'jest-environment-jsdom',
  collectCoverage: true,
  moduleNameMapper: {
     'react-markdown': '<rootDir>/node_modules/react-markdown/lib/index.js',
  },
}

module.exports = createJestConfig(customJestConfig)