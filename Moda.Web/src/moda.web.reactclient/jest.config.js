const nextJest = require('next/jest')

const createJestConfig = nextJest({
  dir: './'
})

const customJestConfig = {
  testEnvironment: 'jest-environment-jsdom',
  collectCoverage: true,
}

module.exports = createJestConfig(customJestConfig)